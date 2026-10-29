using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using Weapons;

namespace Characters
{
    [RequireComponent(typeof(ControllerBase), typeof(CharacterBase))]
    public class CombatBase : MonoBehaviour
    {
        [SerializeField] protected WeaponData testWeapon;
        [SerializeField] protected Transform bulletOrigin; //This is currently moved around by the designated weapon origin of the scriptable object. Remove this middleman operation later.


        public float MaxRange => testWeapon?.maxRange ?? 0f;
        
        private ControllerBase _controller;
        protected CharacterBase Character;

        protected Weapon Weapon;
        private LineRenderer _lineRenderer;


        public void Initialize(ControllerBase controller)
        {
            _controller = controller;
            Character = GetComponent<CharacterBase>();
            _lineRenderer = GetComponent<LineRenderer>();
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

        private void OnDisable()
        {
            Weapon?.FireEvent.RemoveListener(OnFire);
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
            print($"{Weapon.Bullets} / {Weapon.MagazineSize}");

            // Animations later
        }

        protected void OnFire()
        {
            InterpretRaycast(_controller.GetRaycast(Weapon.MaxRange));
        }
        

        private void InterpretRaycast((RaycastHit? hit, Ray ray) raycast)
        {
            if (raycast.hit.HasValue && raycast.hit.Value.collider.transform.TryGetComponent(out BodyPart bodyPart))
            {
                bodyPart.Damage((int)((Weapon.Damage + Character.Stats.DamageAdded) * Character.Stats.DamageMultiplier), Weapon.KnockBack,
                    transform.position);
                DisplayBullet(bulletOrigin.position, raycast.hit.Value.point, 0.05f).Forget();
            }
            else
            {
                DisplayBullet(bulletOrigin.position, bulletOrigin.position + raycast.ray.direction * Weapon.Range,
                    0.05f).Forget();
            }
        }


        private async UniTask DisplayBullet(Vector3 origin, Vector3 target, float delay)
        {
            _lineRenderer.SetPositions(new[] { origin, target });
            _lineRenderer.startColor = Character.Stats.Damage;
            _lineRenderer.endColor = Character.Stats.Damage;
            _lineRenderer.enabled = true;
            await UniTask.WaitForSeconds(delay);
            _lineRenderer.enabled = false;
            await UniTask.Yield();
        }
    }
}