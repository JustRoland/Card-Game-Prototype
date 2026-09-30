using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Weapons;

namespace Characters
{
    [RequireComponent(typeof(ControllerBase), typeof(CharacterBase))]
    public class CombatBase : MonoBehaviour
    {
        [SerializeField] protected WeaponData testWeapon;
        [SerializeField] protected Transform bulletOrigin;

        public float MaxRange => testWeapon?.maxRange ?? 0f;
        
        private ControllerBase _controller;
        private CharacterBase _character;

        private Weapon _weapon;
        private LineRenderer _lineRenderer;
        private Color _color;


        public void Initialize(ControllerBase controller)
        {
            _controller = controller;
            _character = GetComponent<CharacterBase>();
            _lineRenderer = GetComponent<LineRenderer>();
            _weapon = testWeapon.GenerateWeapon();
            _weapon.FireEvent.AddListener(OnFire);
            bulletOrigin.localPosition = _weapon.BulletOrigin;
        }

        private bool ChangeWeapon()
        {
            if (_weapon == null) return false;
            if (_weapon == testWeapon.GenerateWeapon()) return true;
            _weapon.FireEvent.RemoveListener(OnFire);
            _weapon = testWeapon.GenerateWeapon();
            _weapon.FireEvent.AddListener(OnFire);
            bulletOrigin.localPosition = _weapon.BulletOrigin;
            return true;
        }

        private void OnDisable()
        {
            _weapon?.FireEvent.RemoveListener(OnFire);
        }

        public void Attack(bool pressedThisFrame)
        {
            // !! TEMP FOR TESTING !! -- checks and updates what weapon is assigned in Editor to allow hot-swapping. 
            if (!ChangeWeapon()) return;
            
            switch (_weapon.Bullets)
            {
                case 0:
                    Reload();
                    break;
                case > 0:
                    _weapon.Fire(pressedThisFrame);
                    break;
                case -1:
                    _weapon.Fire(pressedThisFrame);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected void Reload()
        {
            if (_weapon == null) return;
            _weapon.Reload();
            print($"{_weapon.Bullets} / {_weapon.MagazineSize}");

            // Animations later
        }

        protected void OnFire()
        {
            InterpretRaycast(_controller.GetRaycast(_weapon.MaxRange));
        }
        
        public void SetColor(Color color) => _color = color;

        private void InterpretRaycast((RaycastHit? hit, Ray ray) raycast)
        {
            if (raycast.hit.HasValue && raycast.hit.Value.collider.transform.TryGetComponent(out BodyPart bodyPart))
            {
                bodyPart.Damage((int)(_weapon.Damage * _character.Stats.DamageMultiplier), _weapon.KnockBack,
                    transform.position);
                DisplayBullet(bulletOrigin.position, raycast.hit.Value.point, 0.05f, _color).Forget();
            }
            else
            {
                DisplayBullet(bulletOrigin.position, bulletOrigin.position + raycast.ray.direction * _weapon.Range,
                    0.05f,
                    _color).Forget();
            }
        }


        private async UniTask DisplayBullet(Vector3 origin, Vector3 target, float delay, Color color)
        {
            _lineRenderer.SetPositions(new[] { origin, target });
            _lineRenderer.startColor = color;
            _lineRenderer.endColor = color;
            _lineRenderer.enabled = true;
            await UniTask.WaitForSeconds(delay);
            _lineRenderer.enabled = false;
            await UniTask.Yield();
        }
    }
}