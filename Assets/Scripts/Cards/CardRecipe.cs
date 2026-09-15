using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Effect
{
    public StatType statType;
    public int value;
    public float duration;
}


[CreateAssetMenu(fileName = "Card Recipe", menuName = "Cards/Card Recipe")]
public class CardRecipe : ScriptableObject
{
    [field: SerializeField] public CardData[] Input { get; private set; }
    [field: SerializeField] public CardData Output { get; private set; }
    
    [field: SerializeField] public Effect[] Effects { get; private set; }
    
    public Card[] InputCards => GenerateCardInputs();
    private Card[] _inputCards;


    private Card[] GenerateCardInputs()
    {
        if (_inputCards != null) return _inputCards;
        if (Input == null) return null;
        
        _inputCards = new Card[Input.Length];
        for (int i = 0; i < _inputCards.Length; i++)
        { 
            _inputCards[i] = Input[i].GenerateCard();
        }
        return _inputCards;
    }
    
}