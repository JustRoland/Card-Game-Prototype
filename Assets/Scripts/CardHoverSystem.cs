using UnityEngine;

public class CardHoverSystem : MonoBehaviour
{

    [SerializeField] private CardView hoverCardView;

    private Vector3 _offset;
    
    public void Initialize(Vector3 positionOffset)
    {
        _offset = positionOffset;
    }
    
    public void Show(CardView card, Vector3 originalPosition)
    {
        hoverCardView.Setup(card.Card, originalPosition + _offset).SelectCard(card.Selected);
        hoverCardView.gameObject.SetActive(true);
    }

    public void Hide() => hoverCardView.gameObject.SetActive(false);
}
