using Cysharp.Threading.Tasks;
using Movement;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private WeaponData testWeapon;
    [SerializeField] private Transform bulletOrigin;

    private Weapon _weapon;
    private PlayerController _playerController;
    private LineRenderer _lineRenderer;

    public void Initialize(PlayerController playerController)
    {
        _playerController = playerController;
        _lineRenderer = GetComponent<LineRenderer>();
        _weapon = testWeapon.GenerateWeapon();
        _weapon.FireEvent.AddListener(OnFire);
        bulletOrigin.localPosition = _weapon.BulletOrigin;
    }

    public void UpdateInput(CharacterInput input)
    {
        if (input.Attack) Attack(input.AttackButtonDown);
        if (input.Reload) Reload();
    }

    private void ChangeWeapon()
    {
        if (_weapon == null) return;
        if (_weapon == testWeapon.GenerateWeapon()) return;
        _weapon.FireEvent.RemoveListener(OnFire);
        _weapon = testWeapon.GenerateWeapon();
        _weapon.FireEvent.AddListener(OnFire);
        bulletOrigin.localPosition = _weapon.BulletOrigin;
    }

    private void Attack(bool pressedThisFrame)
    {
        //TEMP FOR TESTING
        ChangeWeapon();
        //END TEMP FOR TESTING

        _weapon?.Fire(pressedThisFrame);
    }

    private void Reload()
    {
        if (_weapon == null) return;
        _weapon.Reload();
        print($"{_weapon.Bullets} / {_weapon.MagazineSize}");

        // Animations later
    }

    private void OnFire()
    {
        InterpretRaycast(_playerController.CenterScreenRaycast(_weapon.Range));
    }

    private void InterpretRaycast((RaycastHit? hit, Ray ray) raycast)
    {
        if (raycast.hit.HasValue && raycast.hit.Value.transform.TryGetComponent(out BodyPart bodyPart))
        {
            bodyPart.Damage((int)(_weapon.Damage * _playerController.Stats.DamageMultiplier), _weapon.KnockBack, transform.position);
            DisplayBullet(bulletOrigin.position, raycast.hit.Value.point, 0.05f, Color.red).Forget();
        }
        else
        {
            DisplayBullet(bulletOrigin.position, bulletOrigin.position + raycast.ray.direction * _weapon.Range, 0.05f,
                Color.white).Forget();
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