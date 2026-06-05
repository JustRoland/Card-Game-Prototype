using System;
using UnityEngine;

public class Card : IEquatable<Card>
{
    private readonly CardData _data;

    public string Name => _data.name;
    public string Description => _data.Description;
    public Sprite Sprite => _data.Sprite;
    public int Rarity { get; }
    public CardType Type { get; }


    public Card(CardData data)
    {
        _data = data;
        Rarity = data.Tier;
        Type = data.Type;
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