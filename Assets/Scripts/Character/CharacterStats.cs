
public enum StatType
{
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
    Damage,
    Defense
}

public class CharacterStats
{
    private readonly BaseStats _baseStats;
    private readonly StatsMediator _mediator;

    public StatsMediator Mediator => _mediator;

    public CharacterStats(StatsMediator mediator, BaseStats baseStats)
    {
        _mediator = mediator;
        _baseStats = baseStats;
    }

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

public float Damage
    {
        get
        {
            var q = new Query(StatType.Damage, _baseStats.damage);
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
}