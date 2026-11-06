using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game;
using Progress_Bar.Resources;
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

        private readonly List<DamageIndicator> _indicators = new();
        private Color _effectColor;
        private PlayerCombat _combat;
        private PlayerController _controller;
        private ProgressBar _healthBar;
        private bool _inDialogue;

        private void Awake()
        {
            base.Initialize();
            _combat = GetComponent<PlayerCombat>();
            _controller = GetComponent<PlayerController>();
            _healthBar = GetComponentInChildren<ProgressBar>();
            _healthBar.ResetProgressBar((int)Stats.MaxHealth);
            _healthBar.SetProgressBar(Stats.Health);
            damageIndicator.gameObject.SetActive(false);
        }

        private void FixedUpdate()
        {
            _indicators.ForEach(i => i.indicator.rotation = AngleToTarget(i.origin));
        }

        public override void Damage(BodyPart bodyPart, int damage, float knockBack, CharacterBase origin, StatusEffect[] effects = null)
        {
            base.Damage(bodyPart, damage, knockBack, origin, effects);

            _healthBar.SetProgressBar(Stats.Health);
            ToggleDamageIndicator(origin.transform.position, cancellationTokenSource.Token).Forget();
            GameManager.Instance.TrackDamageTaken(damage);

            if (Stats.Health > 0) return;
            if (Stats.Revives > 0)
            {
                Revive();
                return;
            }
            
            onDeathEvent.Invoke();
            _controller.LockPlayerControls(true);
            GameManager.Instance.SetKiller(origin.name);
            GameManager.Instance.EndGame();

        }
        
        private async UniTask ToggleDamageIndicator(Vector3 origin, CancellationToken cancellationToken = default)
        {
            
            var indicatorStr = new DamageIndicator(Instantiate(damageIndicator, canvas.transform), origin);
            indicatorStr.indicator.gameObject.SetActive(true);
            _indicators.Add(indicatorStr);
            
            await UniTask.Delay(TimeSpan.FromSeconds(damageIndicatorDuration), cancellationToken: cancellationToken);
            
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

        private void Revive()
        {
            Stats.Health = Stats.MaxHealth;
            GameManager.Instance.TrackRevive();
        }

        public void SetInDialogue(bool locked) => _inDialogue = locked;

    }
    
}
