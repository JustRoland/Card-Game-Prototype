using System;
using Characters;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cards
{
    [Serializable]
    public class Effect
    {
        [Header("Stat Changes")]
        public StatType statType;
        public float value;
        [FormerlySerializedAs("effectType")] [Header("Effect Color")]
        public Category category;
        public Color color;
        [Header("Trait Unlocks")]
        public TraitType trait;
        [Header("Duration")]
        public float duration;
    }


    [CreateAssetMenu(fileName = "Card Recipe", menuName = "Cards/Card Recipe")]
    public class CardRecipe : ScriptableObject
    {
        [field: SerializeField] public string Description { get; private set; }
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
}