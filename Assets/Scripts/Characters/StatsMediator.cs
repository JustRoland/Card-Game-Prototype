using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Characters
{
    public class StatsMediator
    {
        private readonly List<StatModifier> _modifiers = new();
        private readonly List<StatusEffect> _statusEffects = new();

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
        
        public void AddEffect(StatusEffect sEffect)
        {
            if (!_statusEffects.Contains(sEffect))
            {
                _statusEffects.Add(sEffect);
                Queries += sEffect.Handle;
            }
            else _statusEffects.Find(se => se.type == sEffect.type).ResetTimer();

            sEffect.OnDispose += _ =>
            {
                _statusEffects.Remove(sEffect);
                Queries -= sEffect.Handle;
            };
        }
        public bool HasStatusEffect(StatusEffectType type) => _statusEffects.Any(se => se.type == type);
        public void RemoveModifier(int id) => _modifiers.Find(m => m.ID == id)?.Dispose();
        public void RemoveEffect(StatusEffect sEffect) => _statusEffects.Find(se => se.type == sEffect.type)?.Dispose();
        
        public int Update(float deltaTime)
        {
            _modifiers.ForEach(modifier => modifier.Update(deltaTime));
            for (int i = _modifiers.Count - 1; i >= 0; i--)
            {
                if (_modifiers[i].MarkedForRemoval) _modifiers[i].Dispose();
            }

            int tickDamage = 0;
            
            _statusEffects.ForEach(effect =>
            {
                effect.Update(deltaTime);
                if (effect.ApplyTickEffect) tickDamage += effect.TickEffect();
            });
            for (int i = _statusEffects.Count - 1; i >= 0; i--)
            {
                if (_statusEffects[i].MarkedForRemoval) _statusEffects[i].Dispose();
            }
            
            return tickDamage;
        }
        
        
        
    }


    public class Query
    {
        public readonly StatType StatType;
        public readonly StatusEffectType StatusEffectType;
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

        public Query(StatusEffectType statusEffectType, float value)
        {
            StatusEffectType = statusEffectType;
            Value = value;
        }
    }
}