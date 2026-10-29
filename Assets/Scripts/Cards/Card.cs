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
        public CardType Type => _data.Type;
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
            return other != null && Rarity == other.Rarity && Type == other.Type;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Rarity, (int)Type);
        }
    }

    public enum CardType
    {
        None,
        Fire,
        Rock,
        Wood,
        Elixir
    }
}