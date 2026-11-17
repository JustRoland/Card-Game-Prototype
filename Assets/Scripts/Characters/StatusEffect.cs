using System;
using Utility;

namespace Characters
{

    public class SimpleEffect : StatusEffect
    {
        private readonly Func<float, float> _operation;
        private readonly int _tickHealthChange;
        // private readonly StatModifier[] _effectedStats;
        
        public SimpleEffect(StatusEffectType type, float duration, Func<float, float> operation, int tickHealthChange = 0, StatusEffectType requirement = StatusEffectType.None) : base(type, duration, requirement)
        {
            _operation = operation;
            _tickHealthChange = tickHealthChange;
            // _effectedStats = effectedStats;
        }

        public override void Handle(object sender, Query query)
        {
            if (query.StatusEffectType == type)
            {
                query.Value = _operation(query.Value);
            }
        }

        public override int TickEffect()
        {
            ApplyTickEffect = false;
            return _tickHealthChange;
        }
    }
    
    
    public abstract class StatusEffect : IDisposable, IEquatable<StatusEffect>
    {
        private static int _id;
        public int ID = _id++;
        public readonly StatusEffectType type;
        public readonly StatusEffectType requirement;
        public string Name => type.ToString();
        public bool MarkedForRemoval { get; private set;}
        public bool ApplyTickEffect { get; protected set; }
        public Action<StatusEffect> OnDispose = delegate { };
        private readonly SimpleTimer timer;
        
        
        protected StatusEffect(StatusEffectType type, float duration, StatusEffectType requirement = StatusEffectType.None)
        {
            this.type = type;
            this.requirement = requirement;
            if (duration <= 0) return;
            timer = new SimpleTimer(TimerType.Countdown);
            timer.SetTimer(0, 0, duration);
            timer.CountdownFinishedEvent.AddListener(() => MarkedForRemoval = true);
            timer.SecondEvent.AddListener(() =>
            {
                ApplyTickEffect = true;
            });
            timer.StartTimer();
        }
        
        public void ResetTimer() => timer.ResetTimer();

        public void Update(float deltaTime) => timer?.UpdateTimer(deltaTime);

        public abstract void Handle(object sender, Query query);

        public abstract int TickEffect();

        public void Dispose()
        {
            OnDispose.Invoke(this);
        }

        public bool Equals(StatusEffect other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return type == other.type;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((StatusEffect)obj);
        }

        public override int GetHashCode()
        {
            return (int)type;
        }
    }

}
