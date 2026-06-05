using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject container;
    [SerializeField] private GameObject silhouette;

    private Vector3 _offsetFromPointerPosition;
    private Vector3 _dragStartPosition;
    private Quaternion _dragStartRotation;

    
    [Header("Events")]
    public UnityEvent<CardView, Vector3> StartDrag = new();
    public UnityEvent<CardView, Vector3> Dragging = new();
    public UnityEvent<CardView, Vector3> EndDrag = new();
    public UnityEvent<CardView, Vector3> Click = new();
    public UnityEvent<CardView, Vector3> HoverEnter = new();
    public UnityEvent<CardView, Vector3> HoverExit = new();


    public Card Card { get; private set; }
    public bool Active { get; private set; }
    public bool Selected { get; private set; }

    public CardView Setup(Card card, Vector3 position, float scaleUpTime = 0)
    {
        transform.position = position;

        if (scaleUpTime > 0)
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, scaleUpTime);
        }

        Card = card;
        nameText.text = card.Name;
        descriptionText.text = card.Description;
        valueText.text = card.Rarity.ToString();
        spriteRenderer.sprite = card.Sprite;

        SetActive(true);
        SelectCard(false);

        return this;
    }


    public void SetActive(bool active)
    {
        Active = active;
        gameObject.SetActive(active);
    }

    public void SelectCard(bool selected)
    {
        Selected = selected;
        silhouette.SetActive(selected);
    }

    private void SelectAndAdd(bool selected)
    {
        SelectCard(selected);
        if (selected) CardManager.Instance.SelectedCards.Add(this);
        else CardManager.Instance.SelectedCards.Remove(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CardManager.Instance.IsDragging) return;
        container.SetActive(false);
        HoverEnter.Invoke(this, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CardManager.Instance.IsDragging) return;
        container.SetActive(true);
        HoverExit.Invoke(this, transform.position);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CardManager.Instance.IsDragging) return;
        SelectAndAdd(!Selected);
        Click.Invoke(this, transform.position);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var pos = Camera.main.ScreenToWorldPoint(eventData.position);
        StartDrag.Invoke(this, pos);
    }

    public void SetDragStartParameters(Vector3 pos)
    {
        _dragStartPosition = transform.position;
        _dragStartRotation = transform.rotation;
        _offsetFromPointerPosition = new Vector3(pos.x, pos.y, 0) - transform.position;
        container.SetActive(true);
    }


    public void OnDrag(PointerEventData eventData)
    {
        var pos = Camera.main.ScreenToWorldPoint(eventData.position);
        Dragging.Invoke(this, pos);
    }

    public void DragCardView(Vector3 pos)
    {
        transform.position = new Vector3(pos.x, pos.y, 0) - _offsetFromPointerPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var pos = Camera.main.ScreenToWorldPoint(eventData.position);
        EndDrag.Invoke(this, pos);

    }

    public void ResetCard()
    {
        transform.position = _dragStartPosition;
        transform.rotation = _dragStartRotation;
    }
}