using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;

    [Header("References")] [SerializeField]
    private CardView cardPrefab;

    [SerializeField] private Transform cardSpawnLocation;

    [SerializeField] private Hand hand;
    public readonly List<CardView> SelectedCards = new();
    public bool IsDragging { get; private set; }

    [Header("Variables")] [SerializeField] private CardData[] cardData;
    [SerializeField] private CardRecipe[] recipes;
    [SerializeField] private int factoryStartBuffer;
    [SerializeField] private int factoryMaxItems;
    [SerializeField] private float cardScaleUpTime = 0.15f;
    [SerializeField] private int maxDeckSize = 10;
    private int _realDeckSize;

    private GenericFactory<CardView> _cardFactory;

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
        if (PlayCards(cardView.Selected ? SelectedCards.ToArray() : new[] { cardView })) return;
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

    private void OnRequestNewCardView(Hand hnd)
    {
        var cardView = GetCardView();
        hnd.AddCard(cardView, _token).Forget();
    }

    private void OnRequestRemoveCardView(CardView cardView)
    {
        UnloadExistingCardView(cardView);
    }

    private CardView GetCardView()
    {
        int rand = Random.Range(0, cardData.Length);
        return GetCardView(cardData[rand].GenerateCard());
    }

    private CardView GetCardView(Card card)
    {
        if (_realDeckSize <= 0)
        {
            return null;
        }

        var newCard = _cardFactory.GetItem().Setup(card, cardSpawnLocation.position, cardScaleUpTime);
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
            print($"Unloaded card view {cardView.Card.Name}");
        });
        if (SelectedCards.Contains(cardView)) SelectedCards.Remove(cardView);
        DeregisterCardViewEvents(cardView);
        _realDeckSize++;
    }

    #endregion

    #region Card Playing Logic

    private bool PlayCards(CardView[] cards)
    {
        Debug.Assert(cards is { Length: > 0 }, "Card list is empty");
        Debug.Assert(recipes is { Length: > 0 }, "Recipe list is empty");

        var outputCard =
            recipes
                .Where(r => r.InputCards.SequenceEqual(cards.Select(c => c.Card)))
                .Select(r => r.Output.GenerateCard())
                .FirstOrDefault();

        if (outputCard == null) return false;
        UnloadExistingCardViews(cards);
        hand.RemoveCard(cards, Source.Token).Forget();
        hand.AddCard(GetCardView(outputCard), _token).Forget();
        return true;
    }

    #endregion
}