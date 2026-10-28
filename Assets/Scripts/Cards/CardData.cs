using System;
using Characters;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cards
{
    [CreateAssetMenu(fileName = "Card Data", menuName = "Cards/Card Data")]
    public class CardData : ScriptableObject
    {
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public int Tier { get; private set; }
        [field: SerializeField] public Sprite Sprite { get; private set; }
        [field: SerializeField] public CardType Type { get; private set; }
        
        [field: SerializeField] public Effect[] ActiveEffects { get; private set; }
        [field: SerializeField] public Effect[] PassiveEffects { get; private set; }


        public Card GenerateCard() => new (this);
    }
    
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
}