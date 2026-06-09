using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class Weapon : IEquatable<Weapon>
{
    private readonly WeaponData _data;

    private static int _id;
    public int WeaponID { get; private set; }
    public string Name => _data.name;
    public string Description => _data.description;
    public Sprite Icon => _data.icon;
    public GameObject Model => _data.model;
    public GameObject Projectile => _data.projectile;
    public Vector3 BulletOrigin => _data.bulletOrigin;
    public float Damage => _data.damage;
    public float KnockBack => _data.knockBack;
    public float Range => _data.range;
    public float DamageDropOffRange => _data.damageDropOffRange;
    public float DamageDropOffRate => _data.damageDropOffRate;
    public WeaponFireType FireType => _data.fireType;
    public float FireRate => _data.fireRate;
    public float BurstFireRate => _data.burstFireRate;
    public int BurstSize => _data.burstSize;
    public int MagazineSize => _data.magazineSize;
    public int MaxAmmo => _data.maxAmmo;
    public float ReloadSpeed => _data.reloadSpeed;
    public int Bullets { get; private set; }
    public bool CanFire { get; private set; }

    public readonly UnityEvent FireEvent = new();

    public bool Equals(Weapon other) => other != null && WeaponID == other.WeaponID;
    
    public Weapon(WeaponData data)
    {
        _data = data;
        WeaponID = _id++;
        CanFire = true;
        Bullets = MagazineSize;
    }
    

    public void Fire(bool pressedThisFrame)
    {
        if (!CanFire) return;
        if (Bullets <= 0) return;

        switch (FireType)
        {
            case WeaponFireType.SemiAutomatic when pressedThisFrame:
            case WeaponFireType.FullAuto:
                HandleFiring();
                FireRateTimer().Forget();
                return;
            case WeaponFireType.Burst when pressedThisFrame:
            case WeaponFireType.BurstAuto:
                BurstFire().Forget();
                return;
            case WeaponFireType.BoltAction when pressedThisFrame:
                HandleFiring();
                FireRateTimer().Forget();
                return;
            default:
                return;
        }
    }

    private void HandleFiring()
    {
        Debug.Assert(Bullets > 0, $"Bullets need to be more than 0. (current: {Bullets})");
        Bullets--;
        FireEvent.Invoke();
    }

    public void Reload()
    {
        Bullets = MagazineSize;
    }


    private async UniTask BurstFire()
    {
        Debug.Assert(BurstSize > 1, $"BurstFire size must be greater than 1. (current: {BurstSize})");
        Debug.Assert(BurstFireRate > 0, $"BurstFireRate must be greater than 0. (current: {BurstFireRate})");
        CanFire = false;
        var delay = 60 / BurstFireRate;
        for (var i = 0; i < BurstSize; i++)
        {
            if (Bullets <= 0) break;
            HandleFiring();
            await UniTask.WaitForSeconds(delay);
        }

        await FireRateTimer();
        await UniTask.Yield();
    }

    private async UniTask FireRateTimer()
    {
        if (Bullets > 0)
        {
            CanFire = false;
            var delay = 60 / FireRate;
            await UniTask.WaitForSeconds(delay);
        }

        CanFire = true;
        await UniTask.Yield();
    }


}

public enum WeaponFireType
{
    SemiAutomatic,
    FullAuto,
    Burst,
    BurstAuto,
    BoltAction,
}