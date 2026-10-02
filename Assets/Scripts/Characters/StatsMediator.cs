using System;
using System.Collections.Generic;
using Cards;
using UnityEngine;

namespace Characters
{
    public class StatsMediator
    {
        private readonly List<StatModifier> _modifiers = new();

        public event EventHandler<Query> Queries;
        public void PerformQuery(object sender, Query query) => Queries?.Invoke(sender, query);
    
        public void AddModifiers(StatModifier modifier)
        {
            _modifiers.Add(modifier);
            Queries += modifier.Handle;

            modifier.OnDispose += _ =>
            {
                _modifiers.Remove(modifier);
                Queries -= modifier.Handle;
            };
        }

        public void Update(float deltaTime)
        {
            _modifiers.ForEach(modifier => modifier.Update(deltaTime));
            for (int i = _modifiers.Count - 1; i >= 0; i--)
            {
                if (_modifiers[i].MarkedForRemoval) _modifiers[i].Dispose();
            }
        }
    }


    //TODO: Effect color will likely be added to the query.
    public class Query
    {
        public readonly StatType StatType;
        public readonly Category Category;
        public Color Color;
        public float Value;

        public Query(StatType statType, float value)
        {
            StatType = statType;
            Value = value;
        }
        public Query(Category category, Color color)
        {
            Category = category;
            Color = color;
        }
    }
}