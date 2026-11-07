using System;
using UnityEngine;

namespace Cards
{
    public class Card : IEquatable<Card>
    {
        private readonly CardData _data;

        public string Name => _data.name;
        public string Description => _data.Description;
        public Sprite Sprite => _data.Sprite;
        public int Rarity => _data.Tier;
        public Color Color => _data.Color;
        public Effect[] ActiveEffect => _data.ActiveEffects;
        public Effect[] PassiveEffects => _data.PassiveEffects;
        public readonly int[] modifiersID;


        public Card(CardData data)
        {
            _data = data;
            modifiersID = new int[data.PassiveEffects.Length];
        }


        public bool Equals(Card other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(_data, other._data);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Card)obj);
        }

        public override int GetHashCode()
        {
            return (_data != null ? _data.GetHashCode() : 0);
        }
    }

}