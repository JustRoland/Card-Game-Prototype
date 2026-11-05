using System;
using UnityEngine;
using Utility;

namespace Characters
{
    
    
    public class BasicModifier : StatModifier
    {
        private readonly StatType _type;
        private readonly Func<float, float> _operation;

        public BasicModifier(StatType type, float duration, Func<float, float> operation) : base(duration)
        {
            _type = type;
            _operation = operation;
        }

        public override void Handle(object sender, Query query)
        {
            if (query.StatType == _type)
            {
                query.Value = _operation(query.Value);
            }
        }
    }

    public class ColorModifier : StatModifier
    {
        private readonly Category _type;
        private readonly Color _effectColor;
        
        public ColorModifier(Category type, Color effectColor, float duration) : base(duration)
        {
            _type = type;
            _effectColor = effectColor;
        }

        public override void Handle(object sender, Query query)
        {
            if (query.Category == _type)
            {
                query.Color = _effectColor;
            }
        }
    }

    public abstract class StatModifier : IDisposable
    {
        private static int _id;
        public int ID = _id++;
        public bool MarkedForRemoval { get; private set;}
        public Action<StatModifier> OnDispose = delegate { };
        private readonly SimpleTimer timer;

        protected StatModifier(float duration)
        {
            if (duration <= 0) return;
            timer = new SimpleTimer(TimerType.Countdown);
            timer.SetTimer(0, 0, duration);
            timer.CountdownFinishedEvent.AddListener(() => MarkedForRemoval = true);
            timer.StartTimer();
        }

        public void Update(float deltaTime) => timer?.UpdateTimer(deltaTime);
        public abstract void Handle(object sender, Query query);

        public void Dispose()
        {
            OnDispose.Invoke(this);
        }
    }
}