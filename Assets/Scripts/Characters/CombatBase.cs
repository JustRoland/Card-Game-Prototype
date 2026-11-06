using System;
using System.Collections.Generic;
using System.Threading;
using Characters.Enemies;
using Cysharp.Threading.Tasks;
using Game;
using UnityEngine;
using Utility;
using Weapons;

namespace Characters
{
    [RequireComponent(typeof(ControllerBase), typeof(CharacterBase))]
    public class CombatBase : MonoBehaviour
    {
        [SerializeField] protected bool debug;
        [SerializeField] protected bool trackDamageDealt;
        [SerializeField] protected WeaponData testWeapon;
        private Collider[] _hitColliders;
        [SerializeField] private int maxExplosiveHits = 10;
        [SerializeField] protected GameObject explosion;

        [SerializeField] protected Transform
            bulletOrigin; //This is currently moved around by the designated weapon origin of the scriptable object. Remove this middleman operation later.


        public float MaxRange => testWeapon?.maxRange ?? 0f;

        private ControllerBase _controller;
        protected CharacterBase Character;

        protected Weapon Weapon;
        private LineRenderer _lineRenderer;

        private GenericFactory<GameObject> _explosionFactory;
        
        protected CancellationTokenSource _cancellationTokenSource;


        public void Initialize(ControllerBase controller, CharacterBase character)
        {
            _hitColliders = new Collider[maxExplosiveHits];
            _controller = controller;
            Character = character;
            _lineRenderer = GetComponent<LineRenderer>();
            _explosionFactory = new GenericFactory<GameObject>(explosion, go => !go.activeSelf, maxItems: 100);
            _cancellationTokenSource = new CancellationTokenSource();
            ChangeWeapon();
        }

        protected virtual bool ChangeWeapon()
        {
            if (!testWeapon) return false;
            if (Weapon == testWeapon.GenerateWeapon()) return true;
            Weapon?.FireEvent.RemoveListener(OnFire);
            Weapon = testWeapon.GenerateWeapon();
            Weapon.FireEvent.AddListener(OnFire);
            bulletOrigin.localPosition = Weapon.BulletOrigin;
            return true;
        }

        protected virtual void OnDisable()
        {
            Weapon?.FireEvent.RemoveListener(OnFire);
            _cancellationTokenSource.Cancel();
        }

        public virtual void Attack(bool pressedThisFrame, float fireRateMultiplier)
        {
            // !! TEMP FOR TESTING !! -- checks and updates what weapon is assigned in Editor to allow hot-swapping. 
            if (!ChangeWeapon()) return;

            switch (Weapon.Bullets)
            {
                case 0:
                    break;
                case > 0:
                    Weapon.Fire(pressedThisFrame, fireRateMultiplier);
                    break;
                case -1:
                    Weapon.Fire(pressedThisFrame, fireRateMultiplier);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual void Reload()
        {
            if (Weapon == null) return;
            Weapon.Reload();

            // Animations later
        }

        protected void OnFire()
        {
            InterpretRaycast(_controller.GetRaycast(Weapon.MaxRange, ~(1 << gameObject.layer)));
        }


        private void InterpretRaycast((RaycastHit? hit, Ray ray) raycast)
        {
            if (!raycast.hit.HasValue)
            {
                DisplayBullet(
                    new[] { bulletOrigin.position, bulletOrigin.position + raycast.ray.direction * Weapon.Range },
                    0.05f, _cancellationTokenSource.Token).Forget();
            }
            else
            {
                //TODO: Clean up; very brute force and not expandable.
                List<StatusEffect> effects = new List<StatusEffect>();
                //Using the reference to the variable when creating the status effect has the neat side effect of automatically updating the damage multiplier value on the host. Cache value first if this is unintended.
                if (Character.Stats.HasTrait(TraitType.Weaken))
                    effects.Add(new SimpleEffect(StatusEffectType.Weakened, 3f,
                        v => v + Character.Stats.WeakenedDamageMultiplier));


                Vector3 impactPosition = raycast.hit.Value.point;
                List<Vector3> points = new List<Vector3> {bulletOrigin.position, impactPosition};
                
                if (Character.Stats.HasTrait(TraitType.ExplosiveShot))
                {
                    ExplosiveDamage(impactPosition, effects.ToArray());
                    return;
                }

                if (raycast.hit.Value.collider.transform.TryGetComponent(out BodyPart bodyPart))
                {
                    if (debug) Debug.Log($"{name} hit {bodyPart.Name}");

                    RegularDamage(bodyPart, effects.ToArray());

                    if (Character.Stats.HasTrait(TraitType.Pierce))
                        PiercingShot(raycast.hit.Value.collider.transform.position, raycast.ray.direction, points, effects.ToArray());
                }
                else if (Character.Stats.HasTrait(TraitType.Ricochet))
                {
                    RicochetShot(raycast.hit.Value.point, raycast.ray.direction, raycast.hit.Value.normal, points, effects.ToArray());
                }

                DisplayBullet(points.ToArray(), 0.05f, _cancellationTokenSource.Token).Forget();
            }
        }

        private void RegularDamage(BodyPart bodyPart, StatusEffect[] effects)
        {
            var damage = (int)((Weapon.Damage + Character.Stats.DamageAdded) * Character.Stats.DamageMultiplier);
            bodyPart.Damage(damage,
                Weapon.KnockBack,
                Character,
                effects);
            if (trackDamageDealt) GameManager.Instance.TrackDamageDealt(damage);
        }

        private void ExplosiveDamage(Vector3 position, StatusEffect[] effects)
        {
            var size = Physics.OverlapSphereNonAlloc(position, Character.Stats.ExplosiveShotRadius, _hitColliders);

            ExplosiveShotEffect(position, Character.Stats.ExplosiveShotRadius, 0.075f, _cancellationTokenSource.Token).Forget();

            var hitCharacters = new List<CharacterBase>();

            for (var i = 0; i < size; i++)
            {
                if (!_hitColliders[i].TryGetComponent(out BodyPart explosionTarget)) continue;
                if (hitCharacters.Contains(explosionTarget.Character)) continue;
                RegularDamage(explosionTarget, effects);
                hitCharacters.Add(explosionTarget.Character);
            }
        }

        private async UniTask ExplosiveShotEffect(Vector3 position, float radius, float duration, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                foreach (var item in _explosionFactory.ActiveItems)
                {
                    item.SetActive(false);
                }

                return;
            }
            
            var explosive = _explosionFactory.GetItem();
            explosive.transform.position = position;
            explosive.transform.localScale = new Vector3(radius, radius, radius);
            explosive.SetActive(true);
            await UniTask.WaitForSeconds(duration, cancellationToken: cancellationToken);
            explosive.SetActive(false);
            await UniTask.Yield();
        }


        private async UniTask DisplayBullet(Vector3[] positions, float delay, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _lineRenderer.enabled = false;
                return;
            }
            
            if (!_lineRenderer) return;
            _lineRenderer.positionCount = positions.Length;
            _lineRenderer.SetPositions(positions);
            _lineRenderer.startColor = Character.Stats.Damage;
            _lineRenderer.endColor = Character.Stats.Damage;
            _lineRenderer.enabled = true;
            await UniTask.WaitForSeconds(delay, cancellationToken: cancellationToken);
            _lineRenderer.enabled = false;
            await UniTask.Yield();
        }

        private void PiercingShot(Vector3 position, Vector3 direction, List<Vector3> points,
            StatusEffect[] effects)
        {
            var pierceCount = Character.Stats.PierceCount;
            while (pierceCount > 0)
            {
                if (!Physics.Raycast(position, direction, out var hit, Character.Stats.MaxRange,
                        ~(1 << gameObject.layer))) return;

                if (!hit.collider.TryGetComponent(out BodyPart bodyPart))
                {
                    RicochetShot(hit.point, direction, hit.normal, points, effects);
                    return;
                }

                points.Add(hit.point);
                RegularDamage(bodyPart, effects);

                pierceCount--;
                position = hit.transform.position;
            }
        }

        private void RicochetShot(Vector3 position, Vector3 direction, Vector3 surfaceNormal, List<Vector3> points, StatusEffect[] effects)
        {
            var bounces = Character.Stats.RicochetCount;
            while (bounces > 0)
            {
                var ricochetDirection = direction - 2 * Vector3.Dot(direction, surfaceNormal) * surfaceNormal;

                if (!Physics.Raycast(position, ricochetDirection, out var hit, Character.Stats.MaxRange,
                        ~(1 << gameObject.layer)))
                {
                    points.Add(position + ricochetDirection * Character.Stats.MaxRange);
                    return;
                }

                if (!hit.collider.TryGetComponent(out BodyPart bodyPart))
                {
                    points.Add(hit.point);
                    return;
                }

                points.Add(hit.point);
                if (Character.Stats.HasTrait(TraitType.Pierce))
                    PiercingShot(hit.point, ricochetDirection, points, effects);
                else RegularDamage(bodyPart, effects);

                bounces--;
                position = hit.point;
                direction = ricochetDirection;
                surfaceNormal = hit.normal;
            }
        }
    }
}