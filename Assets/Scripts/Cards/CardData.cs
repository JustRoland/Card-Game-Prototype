using System;
using Characters;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Weapons;

namespace Cards
{
    [CreateAssetMenu(fileName = "Card Data", menuName = "Cards/Card Data")]
    public class CardData : ScriptableObject, IEquatable<CardData>
    {
        private static int _id;
        public readonly int cardID = _id++;
        [Title("Card Description", bold: false)] [HideLabel] [MultiLineProperty(6)]
        [SerializeField] private string description;
        [Title("Card Properties", bold: false)]
        [SerializeField] private int tier = 1;
        [SerializeField] private Sprite sprite;
        [SerializeField] private Color color;
        [PropertySpace(6)][Title("Card Effects", bold: false)]
        [SerializeField] private Effect[] activeEffects;
        [SerializeField] private Effect[] passiveEffects;
        
        public string Description => description;
        public int Tier => tier;
        public Sprite Sprite => sprite;
        public Color Color => color;
        public Effect[] ActiveEffects => activeEffects;
        public Effect[] PassiveEffects => passiveEffects;


        public Card GenerateCard() => new (this);


        public bool Equals(CardData other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && cardID == other.cardID;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((CardData)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), cardID);
        }
    }
    
    [Serializable]
    public class Effect
    {
        [Header("Stat Changes")]
        public StatType statType;
        public float value;
        [Header("Health Changes")]
        public int healthChange;
        [FormerlySerializedAs("effectType")] [Header("Effect Color")]
        public Category category;
        public Color color;
        [Header("Trait Unlocks")]
        public TraitType trait;
        [Header("Duration")]
        public float duration;
        [Header("Weapon Unlock")] public WeaponData weaponData;
    }
}