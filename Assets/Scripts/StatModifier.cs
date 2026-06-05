using System;


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

public abstract class StatModifier : IDisposable
{
    public bool MarkedForRemoval { get; private set;}
    public Action<StatModifier> OnDispose = delegate { };

    public readonly SimpleTimer Timer;

    protected StatModifier(float duration)
    {
        if (duration <= 0) return;
        Timer = new SimpleTimer(TimerType.Countdown);
        Timer.SetTimer(0, 0, duration);
        Timer.CountdownFinishedEvent.AddListener(() => MarkedForRemoval = true);
        Timer.StartTimer();
    }

    public void Update(float deltaTime) => Timer?.UpdateTimer(deltaTime);
    public abstract void Handle(object sender, Query query);

    public void Dispose()
    {
        OnDispose.Invoke(this);
    }
}