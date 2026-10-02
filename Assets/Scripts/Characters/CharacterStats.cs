using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Characters
{
    public enum StatType
    {
        None,
        Acceleration,
        WalkSpeed,
        CrouchSpeed,
        SprintSpeed,
        SlowedSpeed,
        JumpForce,
        AirAcceleration,
        DashForce,
        DashCooldown,
        DashAirDistance,
        Health,
        DamageMultiplier,
        Defense,
        TurnSpeed,
        MaxRange,
    }
    

    public enum Category
    {
        None,
        Damage,
        Heal,
    }

    public class CharacterStats
    {
        private readonly BaseStats _baseStats;
        private readonly StatsMediator _mediator;
        private readonly Color _baseColor;
        private readonly List<Trait> _traits = new();

        public StatsMediator Mediator => _mediator;

        public CharacterStats(StatsMediator mediator, BaseStats baseStats, BaseTraits baseTraits, Color baseColor)
        {
            _mediator = mediator;
            _baseStats = baseStats;
            _baseColor = baseColor;
            foreach (var trait in baseTraits.traits)
            {
                _traits.Add(new Trait(trait.type, trait.unlocked));
            }
        }

        #region Stats

        public float Acceleration
        {
            get
            {
                var q = new Query(StatType.Acceleration, _baseStats.acceleration);
                _mediator.PerformQuery(this, q);
                return q.Value;
            }
        }

        public float WalkSpeed
        {
            get
            {
                var q = new Query(StatType.WalkSpeed, _baseStats.walkSpeed);
                _mediator.PerformQuery(this, q);
                return q.Value;
            }
        }

        public float CrouchSpeed
        {
            get
            {
                var q = new Query(StatType.CrouchSpeed, _baseStats.crouchSpeed);
                _mediator.PerformQuery(this, q);
                return q.Value;
            }
        }

        public float SprintSpeed
        {
            get
            {
                var q = new Query(StatType.SprintSpeed, _baseStats.sprintSpeed);
                _mediator.PerformQuery(this, q);
                return q.Value;
            }
        }

        public float SlowedSpeed
        {
            get
            {
                var q = new Query(StatType.SlowedSpeed, _baseStats.slowedSpeed);
                _mediator.PerformQuery(this, q);
                return q.Value;
            }
        }

        public float JumpForce
        {
            get
            {
                var q = new Query(StatType.JumpForce, _baseStats.jumpForce);
                _mediator.PerformQuery(this, q);
                return q.Value;
            }
        }

        public float AirAcceleration
        {
            get
            {
                var q = new Query(StatType.AirAcceleration, _baseStats.airAcceleration);
                _mediator.PerformQuery(this, q);
                return q.Value;
            }
        }

        public float DashForce
        {
            get
            {
                var q = new Query(StatType.DashForce, _baseStats.dashForce);
                _mediator.PerformQuery(this, q);
                return q.Value;
            }
        }

        public float DashCooldown
        {
            get
            {
                var q = new Query(StatType.DashCooldown, _baseStats.dashCooldown);
                _mediator.PerformQuery(this, q);
                return q.Value;
            }
        }

        public float DashAirDistance
        {
            get
            {
                var q = new Query(StatType.DashAirDistance, _baseStats.dashAirDistance);
                _mediator.PerformQuery(this, q);
                return q.Value;
            }
        }

        public float Health
        {
            get
            {
                var q = new Query(StatType.Health, _baseStats.health);
                _mediator.PerformQuery(this, q);
                return q.Value;
            }
        }

        public float DamageMultiplier
        {
            get
            {
                var q = new Query(StatType.DamageMultiplier, _baseStats.damageMultiplier);
                _mediator.PerformQuery(this, q);
                return q.Value;
            }
        }

        public float Defense
        {
            get
            {
                var q = new Query(StatType.Defense, _baseStats.defense);
                _mediator.PerformQuery(this, q);
                return q.Value;
            }
        }

        public float TurnSpeed
        {
            get
            {
                var q = new Query(StatType.TurnSpeed, _baseStats.turnSpeed);
                _mediator.PerformQuery(this, q);
                return q.Value;
            }
        }

        public float MaxRange
        {
            get
            {
                var q = new Query(StatType.MaxRange, _baseStats.maxRange);
                _mediator.PerformQuery(this, q);
                return q.Value;
            }
        }

        #endregion

        #region Colors

        public Color Damage
        {
            get
            {
                var q = new Query(Category.Damage, _baseColor);
                _mediator.PerformQuery(this, q);
                return q.Color;
            }
        }

        public Color Heal
        {
            get
            {
                var q = new Query(Category.Heal, _baseColor);
                _mediator.PerformQuery(this, q);
                return q.Color;
            }
        }

        #endregion

        #region Traits

        public bool Trait(TraitType trait) => _traits.Find(t => t.type == trait).unlocked;
        public void UnlockTrait(TraitType trait) => _traits.Find(t => t.type == trait).unlocked = true;
        public void LockTrait(TraitType trait) => _traits.Find(t => t.type == trait).unlocked = false;
        public void UnlockAllTraits() => _traits.ForEach(t => t.unlocked = true);

        #endregion
    }


}