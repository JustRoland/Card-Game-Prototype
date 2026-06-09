using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Weapon Data", menuName = "Weapons Data")]
public class WeaponData : ScriptableObject
{
    [TextArea(3, 5)]
    public string description;
    [Header("Visuals")]
    public Sprite icon;
    public GameObject model;
    public GameObject projectile;
    public Vector3 bulletOrigin;
    [Header("Stats")]
    public float damage;
    public float knockBack;
    public float range;
    public float damageDropOffRange;
    public float damageDropOffRate;
    public WeaponFireType fireType;
    public float fireRate;
    public float burstFireRate;
    public int burstSize;
    public int magazineSize;
    public int maxAmmo;
    public float reloadSpeed;

    private Weapon _weapon;

    public Weapon GenerateWeapon() => _weapon ??= new Weapon(this);
}
