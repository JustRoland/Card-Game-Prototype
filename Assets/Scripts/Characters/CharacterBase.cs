using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Characters
{
    public abstract class CharacterBase : MonoBehaviour
    {
        [SerializeField] protected BaseStats stats;
        [SerializeField] protected BodyPart[] bodyParts;
        [SerializeField] protected Color damageEffectColor;
        [SerializeField] protected float damageEffectDuration = 0.035f;
    

        protected Material Material;
        protected Rigidbody RigidBody;
        protected Color OriginalColor;
        public CharacterStats Stats { get; private set; }

        protected virtual void OnEnable()
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
        
        private void Update()
        {
            Stats.Mediator.Update(Time.deltaTime);
        }

        public virtual void Damage(BodyPart bodyPart, int damage, float knockBack, Vector3 origin)
        {
            Stats.Mediator.AddModifiers(new BasicModifier(StatType.Health, 0, v => v - damage));

            OnDamageEffect(bodyPart, damageEffectDuration, knockBack, origin).Forget();
        }

        protected virtual async UniTask OnDamageEffect(BodyPart bodyPart, float duration, float knockBack, Vector3 origin)
        {
            var knockBackDirection = (RigidBody.position - origin).normalized;
            RigidBody.AddForce(knockBackDirection * knockBack, ForceMode.Impulse);
            bodyPart.SetColor(damageEffectColor);

            await UniTask.WaitForSeconds(duration);

            bodyPart.ResetColor();
        
            await UniTask.Yield();
        }
    }
}
