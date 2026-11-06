using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Threading;

namespace Characters
{
    public abstract class CharacterBase : MonoBehaviour, IStats, IEquatable<CharacterBase>
    {
        private static int _id;
        public readonly int ID = _id++;
        
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

        protected CancellationTokenSource cancellationTokenSource;


        public virtual void Initialize(int statsRoundScaling = 1)
        {
            Stats = new CharacterStats(new StatsMediator(), stats, baseTraits, Color.white, statsRoundScaling);
            Material = GetComponentInChildren<MeshRenderer>().material;
            RigidBody = GetComponent<Rigidbody>();
            OriginalColor = Material.color;
            cancellationTokenSource = new CancellationTokenSource();

            foreach (var bodyPart in bodyParts)
            {
                bodyPart.Initialize(this);
            }
        }

        protected virtual void OnDisable()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            onDeathEvent.RemoveAllListeners();
        }

        private void Update()
        {
            Stats.Mediator.Update(Time.deltaTime);
        }

        public virtual void Damage(BodyPart bodyPart, int damage, float knockBack, CharacterBase origin,
            StatusEffect[] effects = null)
        {
            if (debug) Debug.Log($"{damage * Stats.Weakened} damage (Weakened Multiplier: {Stats.Weakened}) taken to {bodyPart.name} from {origin} and {effects?.Length} effects applied. Health: {Stats.Health - damage * Stats.Weakened}",this);
            
            float weakenedMultiplier = Stats.Weakened;  //value has to be cached, else the reference updates the modifier even after it was applied.
            Stats.Health -= damage * weakenedMultiplier;

            effects?.ToList().ForEach(se => Stats.Mediator.AddEffect(se));

            OnDamageEffect(bodyPart, damageEffectDuration, knockBack, origin.transform.position).Forget();
            
            
            if (Stats.Health <= 0) onDeathEvent.Invoke();
        }

        protected virtual async UniTask OnDamageEffect(BodyPart bodyPart, float duration, float knockBack,
            Vector3 origin, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                bodyPart.ResetColor();
                return;
            }
            
            if (RigidBody)
            {
                var knockBackDirection = (RigidBody.position - origin).normalized;
                RigidBody.AddForce(knockBackDirection * knockBack, ForceMode.Impulse);
            }

            bodyPart.SetColor(damageEffectColor);

            await UniTask.WaitForSeconds(duration, cancellationToken: cancellationToken);

            bodyPart.ResetColor();

            await UniTask.Yield();
        }

        public bool Equals(CharacterBase other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((CharacterBase)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), ID);
        }
    }
}