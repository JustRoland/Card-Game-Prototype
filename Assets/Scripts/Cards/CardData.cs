using UnityEngine;

[CreateAssetMenu(fileName = "Card Data", menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public int Tier { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field: SerializeField] public CardType Type { get; private set; }


    public Card GenerateCard() => new (this);
}