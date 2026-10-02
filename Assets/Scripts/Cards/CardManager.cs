using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Characters;
using Characters.Player;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utility;

namespace Cards
{
    public class CardManager : MonoBehaviour
    {
        public static CardManager Instance;

        [Header("References")] [SerializeField]
        private CardView cardPrefab;

        [SerializeField] private GameObject cardDropPrefab;

        [SerializeField] private Transform cardSpawnLocation;

        [SerializeField] private Hand hand;
        [SerializeField] private Camera handCamera;
        [SerializeField] private PlayerCharacter player;
        public readonly List<CardView> SelectedCards = new();
        public bool IsDragging { get; private set; }

        [Header("Variables")] [SerializeField] private CardData[] cardData;
        [SerializeField] private CardRecipe[] recipes;
        [SerializeField] private int factoryStartBuffer;
        [SerializeField] private int factoryMaxItems;
        [SerializeField] private float cardScaleUpTime = 0.15f;
        [SerializeField] private int maxDeckSize = 10;
        [SerializeField] private float minCardYDragToPlay = 2f;
        private Vector3 _startDragPosition;
        private int _realDeckSize;

        private GenericFactory<CardView> _cardFactory;
        private GenericFactory<GameObject> _cardGameObjectFactory;

        public CancellationTokenSource Source;
        private CancellationToken _token;


        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            Source = new CancellationTokenSource();
            _token = Source.Token;

            _cardFactory = new GenericFactory<CardView>(cardPrefab, cardView => cardView.Active == false,
                factoryStartBuffer,
                factoryMaxItems);

            if (cardDropPrefab != null)
                _cardGameObjectFactory =
                    new GenericFactory<GameObject>(cardDropPrefab, cardDrop => !cardDrop.activeSelf, 0, 20);

            _realDeckSize = maxDeckSize;
        }

        private void OnEnable()
        {
            hand.RequestCardView.AddListener(OnRequestNewCardView);
            hand.RequestRemoveCardView.AddListener(OnRequestRemoveCardView);

            _cardFactory.LoadedItems.ForEach(RegisterCardViewEvents);
        }

        private void OnDisable()
        {
            hand.RequestCardView.RemoveListener(OnRequestNewCardView);
            hand.RequestRemoveCardView.RemoveListener(OnRequestRemoveCardView);


            _cardFactory.LoadedItems.ForEach(DeregisterCardViewEvents);
        }

        public void GetCardDrop(Vector3 position)
        {
            var drop = _cardGameObjectFactory.GetItem();
            drop.transform.position = position;
            drop.SetActive(true);
        }


        #region Event Listeners & Registration

        private void OnClick(CardView cardView, Vector3 position) => hand.HoverSystem.Show(cardView, position);

        private void OnHoverEnter(CardView cardView, Vector3 position) => hand.HoverSystem.Show(cardView, position);

        private void OnHoverExit(CardView cardView, Vector3 position) => hand.HoverSystem.Hide();

        private void OnStartDrag(CardView cardView, Vector3 position)
        {
            IsDragging = true;
            hand.HoverSystem.Hide();
            _startDragPosition = position;
        
            if (cardView.Selected) SelectedCards.ForEach(c => c.SetDragStartParameters(position));
            else cardView.SetDragStartParameters(position);
        }

        private void OnBeingDragged(CardView cardView, Vector3 position)
        {
            if (cardView.Selected) SelectedCards.ForEach(c => c.DragCardView(position));
            else cardView.DragCardView(position);
        }

        private void OnEndDrag(CardView cardView, Vector3 position)
        {
            IsDragging = false;
            if (PlayCards(cardView.Selected ? SelectedCards.ToArray() : new[] { cardView }, position)) return;
            if (cardView.Selected) SelectedCards.ForEach(c => c.ResetCard());
            else cardView.ResetCard();
        }

        private void RegisterCardViewEvents(CardView cardView)
        {
            cardView.Click.AddListener(OnClick);
            cardView.HoverEnter.AddListener(OnHoverEnter);
            cardView.HoverExit.AddListener(OnHoverExit);
            cardView.StartDrag.AddListener(OnStartDrag);
            cardView.Dragging.AddListener(OnBeingDragged);
            cardView.EndDrag.AddListener(OnEndDrag);
        }

        private void DeregisterCardViewEvents(CardView cardView)
        {
            cardView.Click.RemoveListener(OnClick);
            cardView.HoverEnter.RemoveListener(OnHoverEnter);
            cardView.HoverExit.RemoveListener(OnHoverExit);
            cardView.StartDrag.RemoveListener(OnStartDrag);
            cardView.Dragging.RemoveListener(OnBeingDragged);
            cardView.EndDrag.RemoveListener(OnEndDrag);
        }

        #endregion


        #region Adding & Removig Cards

        public void AddCard()
        {
            hand.AddCard(GetCardView(hand.transform.localScale), _token).Forget();
        }

        private void OnRequestNewCardView(Hand hnd)
        {
            var cardView = GetCardView(hnd.transform.localScale);
            hnd.AddCard(cardView, _token).Forget();
        }

        private void OnRequestRemoveCardView(CardView cardView)
        {
            UnloadExistingCardView(cardView);
        }

        private CardView GetCardView(Vector3 scale)
        {
            int rand = Random.Range(0, cardData.Length);
            return GetCardView(cardData[rand].GenerateCard(), scale);
        }

        private CardView GetCardView(Card card, Vector3 scale)
        {
            if (_realDeckSize <= 0)
            {
                return null;
            }

            var newCard = _cardFactory.GetItem().Setup(card, cardSpawnLocation.position, scale, cardScaleUpTime, handCamera);
            RegisterCardViewEvents(newCard);
            _realDeckSize--;
            return newCard;
        }

        private void UnloadExistingCardViews(CardView[] cards) => cards.ToList().ForEach(UnloadExistingCardView);

        private void UnloadExistingCardView(CardView cardView)
        {
            _cardFactory.UnloadItem(cardView, () =>
            {
                cardView.SetActive(false);
                cardView.SelectCard(false);
            });
            if (SelectedCards.Contains(cardView)) SelectedCards.Remove(cardView);
            DeregisterCardViewEvents(cardView);
            _realDeckSize++;
        }

        #endregion

        #region Card Playing Logic

        private bool PlayCards(CardView[] cards, Vector3 dropPos)
        {
            Debug.Assert(cards is { Length: > 0 }, "Card list is empty");
            Debug.Assert(recipes is { Length: > 0 }, "Recipe list is empty");
        
            if (Mathf.Abs(dropPos.y - _startDragPosition.y) < minCardYDragToPlay) return false;

            var recipe = recipes.FirstOrDefault(r => r.InputCards.SequenceEqual(cards.Select(c => c.Card)));
        
            if (!recipe) return false;

            return CardUpgrade(recipe, cards) || CardEffect(recipe, cards);
        }

        private bool CardUpgrade(CardRecipe recipe, CardView[] cards)
        {
            var outputCard = recipe.Output?.GenerateCard();
        
            if (outputCard == null) return false;

            UnloadExistingCardViews(cards);
            hand.RemoveCard(cards, Source.Token).Forget();
            hand.AddCard(GetCardView(outputCard, hand.transform.localScale), _token).Forget();
        
            return true;
        }

        private bool CardEffect(CardRecipe recipe, CardView[] cards)
        {
            var effects = recipe.Effects;

            if (effects == null) return false;

            foreach (var effect in effects)
            {
                var modifier = new BasicModifier(effect.statType, effect.duration, v => v + effect.value);
                player.Stats.Mediator.AddModifiers(modifier);
                var colorModifier = new ColorModifier(effect.category, effect.color, effect.duration);
                player.Stats.Mediator.AddModifiers(colorModifier);
                player.Stats.UnlockTrait(effect.trait);
            }
        
            UnloadExistingCardViews(cards);
        
            return true;
        }

        #endregion
    }
}