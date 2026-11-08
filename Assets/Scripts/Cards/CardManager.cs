using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Characters;
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
        [SerializeField] private InterfaceReference<IStats> statsHolder;
        public readonly List<CardView> SelectedCards = new();
        public bool IsDragging { get; private set; }

        [Header("Variables")] private CardData[] cardData;
        private CardRecipe[] recipes;
        [SerializeField] private Color foundRecipeColor;
        private Color _originalColor;
        [SerializeField] private int factoryStartBuffer;
        [SerializeField] private int factoryMaxItems;
        [SerializeField] private int maxDeckSize = 10;
        [SerializeField] private float minCardYDragToPlay = 2f;
        private Vector3 _startDragPosition;
        private int _realDeckSize;

        private GenericFactory<CardView> _cardFactory;


        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            cardData = Resources.LoadAll<CardData>("Data/Cards/Tier 1");
            Debug.Assert(cardData.Length > 0, "No cards found");
            Debug.Log($"Loaded {cardData.Length} cards");
            recipes = Resources.LoadAll<CardRecipe>("Data/Cards");
            Debug.Assert(recipes.Length > 0, "No recipes found");
            Debug.Log($"Loaded {recipes.Length} recipes");

            _cardFactory = new GenericFactory<CardView>(cardPrefab, cardView => !cardView.Active,
                factoryStartBuffer,
                factoryMaxItems);

            _originalColor = cardPrefab.SilhouetteColor;

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
            var cardView = GetCardView(hand.transform.localScale);
            hand.AddCard(cardView);
        }

        private void OnRequestNewCardView(Hand hnd)
        {
            var cardView = GetCardView(hnd.transform.localScale);
            hnd.AddCard(cardView);
        }

        private void OnRequestRemoveCardView(Hand hnd, CardView cardView)
        {
            hnd.RemoveCard(new[] { cardView });
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

            var newCard = _cardFactory.GetItem().Setup(card, cardSpawnLocation.position, handCamera);
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
                cardView.SelectCard(false, false);
            });
            if (SelectedCards.Contains(cardView)) SelectedCards.Remove(cardView);
            DeregisterCardViewEvents(cardView);
            _realDeckSize++;
        }

        #endregion

        #region Card Playing Logic

        private bool PlayCards(CardView[] cards, Vector3 dropPos)
        {
            if (Mathf.Abs(dropPos.y - _startDragPosition.y) < minCardYDragToPlay) return false;

            Debug.Assert(cards is { Length: > 0 }, "Card list is empty");
            if (cards.Length == 1) return PlaySingleCard(cards[0]);

            Debug.Assert(recipes is { Length: > 0 }, "Recipe list is empty");
            // var recipe = recipes.FirstOrDefault(r => r.InputCards.SequenceEqual(cards.Select(c => c.Card)));
            var recipe = recipes.FirstOrDefault(r =>
                CompareLists(r.InputCards.ToList(), cards.Select(c => c.Card).ToList()));
            if (!recipe) return false;

            return CardUpgrade(recipe, cards) || CardEffect(recipe, cards);
        }

        private bool PlaySingleCard(CardView cardView)
        {
            if (statsHolder.Value != null)
            {
                foreach (var effect in cardView.Card.ActiveEffect)
                {
                    var modifier = new BasicModifier(effect.statType, effect.duration, v => v + effect.value);
                    statsHolder.Value.Stats.Mediator.AddModifiers(modifier);
                    statsHolder.Value.Stats.Health += effect.healthChange;
                    var colorModifier = new ColorModifier(effect.category, effect.color, effect.duration);
                    statsHolder.Value.Stats.Mediator.AddModifiers(colorModifier);
                    if (effect.trait is not TraitType.None) statsHolder.Value.Stats.UnlockTrait(effect.trait);
                }
            }

            hand.RemoveCard(new[] { cardView });
            UnloadExistingCardView(cardView);
            return false;
        }

        private bool CardUpgrade(CardRecipe recipe, CardView[] cards)
        {
            var outputCard = recipe.Output?.GenerateCard();

            if (outputCard == null) return false;

            UnloadExistingCardViews(cards);
            hand.RemoveCard(cards);
            hand.AddCard(GetCardView(outputCard, hand.transform.localScale));

            return true;
        }

        // TODO: Refactor this; code repetition.
        private bool CardEffect(CardRecipe recipe, CardView[] cards)
        {
            var effects = recipe.Effects;

            if (effects == null) return false;

            if (statsHolder.Value != null)
            {
                foreach (var effect in effects)
                {
                    var modifier = new BasicModifier(effect.statType, effect.duration, v => v + effect.value);
                    statsHolder.Value.Stats.Mediator.AddModifiers(modifier);
                    statsHolder.Value.Stats.Health += effect.healthChange;
                    var colorModifier = new ColorModifier(effect.category, effect.color, effect.duration);
                    statsHolder.Value.Stats.Mediator.AddModifiers(colorModifier);
                    statsHolder.Value.Stats.UnlockTrait(effect.trait);
                }
            }

            UnloadExistingCardViews(cards);

            return true;
        }

        public void CheckForRecipe()
        {
            // if (recipes.FirstOrDefault(r => r.InputCards.SequenceEqual(SelectedCards.Select(c => c.Card))))
            if (recipes.Any(r =>
                    CompareLists(r.InputCards.ToList(), SelectedCards.Select(c => c.Card).ToList())))
            {
                _cardFactory.LoadedItems.ForEach(i => i.SilhouetteColor = foundRecipeColor);
            }
            else
            {
                _cardFactory.LoadedItems.ForEach(i => i.SilhouetteColor = _originalColor);
            }
        }

        #endregion

        private static bool CompareLists<T>(List<T> aListA, List<T> aListB)
        {
            if (aListA == null || aListB == null || aListA.Count != aListB.Count)
                return false;
            if (aListA.Count == 0)
                return true;

            Dictionary<T, int> lookUp = new Dictionary<T, int>();
            // create index for the first list
            for (int i = 0; i < aListA.Count; i++)
            {
                int count = 0;
                if (!lookUp.TryGetValue(aListA[i], out count))
                {
                    lookUp.Add(aListA[i], 1);
                    continue;
                }

                lookUp[aListA[i]] = count + 1;
            }

            for (int i = 0; i < aListB.Count; i++)
            {
                int count = 0;
                if (!lookUp.TryGetValue(aListB[i], out count))
                {
                    // early exit as the current value in B doesn't exist in the lookUp (and not in ListA)
                    return false;
                }

                count--;
                if (count <= 0)
                    lookUp.Remove(aListB[i]);
                else
                    lookUp[aListB[i]] = count;
            }

            // if there are remaining elements in the lookUp, that means ListA contains elements that do not exist in ListB
            return lookUp.Count == 0;
        }
    }
}