using System.Collections.Generic;
using System.Linq;
using Characters;
using UnityEngine;
using Utility;
using Utility.UI_Message;

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
        [SerializeField] private InterfaceReference<IChangeWeaponAccess> weaponHolder;
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

        private void OnLeftClick(CardView cardView, Vector3 position) => hand.HoverSystem.Show(cardView, position);

        private void OnRightClick(CardView cardView, Vector3 position)
        {
            hand.RemoveCard(new[] { cardView });
            UnloadExistingCardView(cardView);
            hand.HoverSystem.Hide();
        }

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

        private void OnEndDragLeftClick(CardView cardView, Vector3 position)
        {
            IsDragging = false;
            if (PlayCards(cardView.Selected ? SelectedCards.ToArray() : new[] { cardView }, position)) return;
            if (cardView.Selected) SelectedCards.ForEach(c => c.ResetCard());
            else cardView.ResetCard();
        }

        private void OnEndDragRightClick(CardView cardView, Vector3 position)
        {
            hand.RemoveCard(new[] { cardView });
            hand.HoverSystem.Hide();
            UnloadExistingCardView(cardView);
        }

        private void RegisterCardViewEvents(CardView cardView)
        {
            cardView.LeftClick.AddListener(OnLeftClick);
            cardView.RightClick.AddListener(OnRightClick);
            cardView.HoverEnter.AddListener(OnHoverEnter);
            cardView.HoverExit.AddListener(OnHoverExit);
            cardView.StartDrag.AddListener(OnStartDrag);
            cardView.Dragging.AddListener(OnBeingDragged);
            cardView.EndDragLeftClick.AddListener(OnEndDragLeftClick);
            cardView.EndDragRightClick.AddListener(OnEndDragRightClick);
        }

        private void DeregisterCardViewEvents(CardView cardView)
        {
            cardView.LeftClick.RemoveListener(OnLeftClick);
            cardView.RightClick.RemoveListener(OnRightClick);
            cardView.HoverEnter.RemoveListener(OnHoverEnter);
            cardView.HoverExit.RemoveListener(OnHoverExit);
            cardView.StartDrag.RemoveListener(OnStartDrag);
            cardView.Dragging.RemoveListener(OnBeingDragged);
            cardView.EndDragLeftClick.RemoveListener(OnEndDragLeftClick);
            cardView.EndDragRightClick.RemoveListener(OnEndDragRightClick);
        }

        #endregion


        #region Adding & Removig Cards

        public bool AddCard()
        {
            if (!hand.CanAddCard)
            {
                MessageManager.Instance?.NewMessage("Hand is full", color: new Color(1, 0, .2f, 1));
                return false;
            }

            var cardView = GetCardView(hand.transform.localScale);
            hand.AddCard(cardView);
            MessageManager.Instance?.NewMessage($"+1 {cardView.Card.Name} card", color: cardView.Card.Color);
            return true;
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
            var rand = Random.Range(0, cardData.Length);
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
                    //TODO: Separate self-target effects from enemy target effects
                    
                    if (!effect.targetOthers)
                    {
                        if (effect.statType is not StatType.None)
                            statsHolder.Value.Stats.Mediator.AddModifiers(new BasicModifier(effect.statType,
                                effect.duration, v => v + effect.value));
                        statsHolder.Value.Stats.Health += effect.healthChange;
                        if (effect.statusEffectType is not StatusEffectType.None)
                            statsHolder.Value.Stats.Mediator.AddEffect(new SimpleEffect(effect.statusEffectType,
                                effect.duration, null));
                        // TODO: Add the option for temporary trait unlocks. This is now a temporary fix.
                        // if (effect.trait is not TraitType.None) statsHolder.Value.Stats.UnlockTrait(effect.trait);
                        if (effect.weaponData) weaponHolder.Value.ChangeWeapon(effect.weaponData);
                        MessageManager.Instance?.NewImage(cardView.Card.Sprite, duration: effect.duration - 0.35f,
                            fadeOutTime: 0.35f);
                    }
                    else
                    {
                        Spells.ApplyEffect.ToAllCharacters(effect.healthChange);
                        if (effect.statType is not StatType.None)
                            Spells.ApplyEffect.ToAllCharacters(new BasicModifier(effect.statType, effect.duration,
                                v => v + effect.value));
                        if (effect.statusEffectType is not StatusEffectType.None)
                            Spells.ApplyEffect.ToAllCharacters(new SimpleEffect(effect.statusEffectType,
                                effect.duration, null, (int)effect.value));
                    }
                }
            }


            MessageManager.Instance?.NewMessage($"{cardView.Card.Name} card played",
                color: cardView.Card.Color);
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
            MessageManager.Instance?.NewMessage($"+1 {outputCard.Name} card", color: outputCard.Color);


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

            var lookUp = new Dictionary<T, int>();
            
            // create index for the first list
            foreach (var t in aListA)
            {
                if (!lookUp.TryGetValue(t, out var count))
                {
                    lookUp.Add(t, 1);
                    continue;
                }

                lookUp[t] = count + 1;
            }

            foreach (var t in aListB)
            {
                if (!lookUp.TryGetValue(t, out var count))
                {
                    // early exit as the current value in B doesn't exist in the lookUp (and not in ListA)
                    return false;
                }

                count--;
                if (count <= 0)
                    lookUp.Remove(t);
                else
                    lookUp[t] = count;
            }

            // if there are remaining elements in the lookUp, that means ListA contains elements that do not exist in ListB
            return lookUp.Count == 0;
        }
    }
}