using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace Characters
{
    public abstract class CharacterBase : MonoBehaviour, IStats
    {
        [SerializeField] protected BaseStats stats;

        [SerializeField, PropertySpace(SpaceBefore = 0, SpaceAfter = 8)]
        protected BaseTraits baseTraits;

        [FoldoutGroup("Damage Effects"), SerializeField]
        protected BodyPart[] bodyParts;

        [FoldoutGroup("Damage Effects"), SerializeField]
        protected Color damageEffectColor;

        [FoldoutGroup("Damage Effects"), SerializeField]
        protected float damageEffectDuration = 0.035f;

        [FoldoutGroup("Events")] public UnityEvent onDeathEvent = new();

        [SerializeField, Space] protected bool debug;

        protected Material Material;
        protected Rigidbody RigidBody;
        protected Color OriginalColor;
        public CharacterStats Stats { get; private set; }

        public virtual void Initialize(int statsRoundScaling = 1)
        {
            Stats = new CharacterStats(new StatsMediator(), stats, baseTraits, Color.white, statsRoundScaling);
            Material = GetComponentInChildren<MeshRenderer>().material;
            RigidBody = GetComponent<Rigidbody>();
            OriginalColor = Material.color;

            foreach (var bodyPart in bodyParts)
            {
                bodyPart.Initialize(this);
            }
        }

        private void Update()
        {
            Stats.Mediator.Update(Time.deltaTime);
        }

        public virtual void Damage(BodyPart bodyPart, int damage, float knockBack, Vector3 origin,
            StatusEffect[] effects = null)
        {
            if (debug) Debug.Log($"{damage * Stats.Weakened} damage (Weakened Multiplier: {Stats.Weakened}) taken to {bodyPart.name} from {origin} and {effects?.Length} effects applied. Health: {Stats.Health - damage * Stats.Weakened}",this);
            
            float weakenedMultiplier = Stats.Weakened;  //value has to be cached, else the reference updates the modifier even after it was applied.
            Stats.Mediator.AddModifiers(new BasicModifier(StatType.Health, 0, v => v - damage * weakenedMultiplier));

            effects?.ToList().ForEach(se => Stats.Mediator.AddEffect(se));

            OnDamageEffect(bodyPart, damageEffectDuration, knockBack, origin).Forget();
            
            
            if (Stats.Health <= 0) onDeathEvent.Invoke();
        }

        protected virtual async UniTask OnDamageEffect(BodyPart bodyPart, float duration, float knockBack,
            Vector3 origin)
        {
            if (RigidBody)
            {
                var knockBackDirection = (RigidBody.position - origin).normalized;
                RigidBody.AddForce(knockBackDirection * knockBack, ForceMode.Impulse);
            }

            bodyPart.SetColor(damageEffectColor);

            await UniTask.WaitForSeconds(duration);

            bodyPart.ResetColor();

            await UniTask.Yield();
        }
    }
}