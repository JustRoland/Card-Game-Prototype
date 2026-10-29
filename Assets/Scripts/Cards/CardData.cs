using System;
using Characters;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cards
{
    [CreateAssetMenu(fileName = "Card Data", menuName = "Cards/Card Data")]
    public class CardData : ScriptableObject
    {
        [Title("Card Description", bold: false)] [HideLabel] [MultiLineProperty(6)]
        [SerializeField] private string description;
        [Title("Card Properties", bold: false)]
        [SerializeField] private int tier = 1;
        [SerializeField] private Sprite sprite;
        [SerializeField] private Color color;
        [SerializeField] private CardType type;
        [PropertySpace(6)][Title("Card Effects", bold: false)]
        [SerializeField] private Effect[] activeEffects;
        [SerializeField] private Effect[] passiveEffects;
        
        public string Description => description;
        public int Tier => tier;
        public Sprite Sprite => sprite;
        public Color Color => color;
        public CardType Type => type;
        public Effect[] ActiveEffects => activeEffects;
        public Effect[] PassiveEffects => passiveEffects;


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