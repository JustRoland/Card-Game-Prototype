using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Characters.Player
{
    public class PlayerCharacter : CharacterBase
    {
        private readonly struct DamageIndicator : IEquatable<DamageIndicator>
        {
            public DamageIndicator(Transform indicator, Vector3 origin)
            {
                this.indicator = indicator;
                this.origin = origin;
            }
            
            public readonly Transform indicator;
            public readonly Vector3 origin;

            public bool Equals(DamageIndicator other)
            {
                return Equals(indicator, other.indicator) && origin.Equals(other.origin);
            }

            public override bool Equals(object obj)
            {
                return obj is DamageIndicator other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(indicator, origin);
            }
        }
        
        [SerializeField] private Transform damageIndicator;
        [SerializeField] private Canvas canvas;
        [SerializeField] private float damageIndicatorDuration = 0.5f;
        CancellationTokenSource _damageIndicatorCancellation;

        private readonly List<DamageIndicator> _indicators = new();
        private Color _effectColor;
        private PlayerCombat _combat;
        private bool _inDialogue;

        private void Awake()
        {
            _combat = GetComponent<PlayerCombat>();
            damageIndicator.gameObject.SetActive(false);
            base.Initialize();
        }

        private void FixedUpdate()
        {
            _indicators.ForEach(i => i.indicator.rotation = AngleToTarget(i.origin));
        }

        public override void Damage(BodyPart bodyPart, int damage, float knockBack, Vector3 origin, StatusEffect[] effects = null)
        {
            base.Damage(bodyPart, damage, knockBack, origin, effects);

            ToggleDamageIndicator(origin).Forget();
        }
        
        private async UniTask ToggleDamageIndicator(Vector3 origin)
        {
            
            var indicatorStr = new DamageIndicator(Instantiate(damageIndicator, canvas.transform), origin);
            indicatorStr.indicator.gameObject.SetActive(true);
            _indicators.Add(indicatorStr);
            
            await UniTask.Delay(TimeSpan.FromSeconds(damageIndicatorDuration));
            
            _indicators.Remove(indicatorStr);
            Destroy(indicatorStr.indicator.gameObject);
            
            await UniTask.Yield();
        }

        private Quaternion AngleToTarget(Vector3 origin)
        {
            origin.y = transform.position.y;
            var direction = (origin - transform.position).normalized;
            var angle = Vector3.SignedAngle(direction, transform.forward, Vector3.up);
            return Quaternion.Euler(0, 0, angle);
        }

        public void SetInDialogue(bool locked) => _inDialogue = locked;

    }
    
}
