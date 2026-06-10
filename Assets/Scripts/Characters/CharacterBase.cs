using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class CharacterBase : MonoBehaviour
{
    [SerializeField] private BaseStats stats;
    [SerializeField] private BodyPart[] bodyParts;
    [SerializeField] protected Color damageEffectColor;
    [SerializeField] protected float damageEffectDuration;
    

    protected Material Material;
    protected Rigidbody RigidBody;
    protected Color OriginalColor;
    public CharacterStats Stats { get; private set; }

    private void OnEnable()
    {
        Stats = new CharacterStats(new StatsMediator(), stats);
        Material = GetComponentInChildren<MeshRenderer>().material;
        RigidBody = GetComponent<Rigidbody>();
        OriginalColor = Material.color;

        foreach (var bodyPart in bodyParts)
        {
            bodyPart.Initialize(this);
        }
    }
    public abstract void Damage(BodyPart bodyPart, int damage, float knockBack, Vector3 origin);

    protected abstract UniTask OnDamageEffect(BodyPart bodyPart, float duration, float knockBack, Vector3 origin);
}
