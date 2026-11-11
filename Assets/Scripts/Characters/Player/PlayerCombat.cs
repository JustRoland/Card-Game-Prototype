using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Weapons;

namespace Characters.Player
{
    public class PlayerCombat : CombatBase
    {
        [Required] [SerializeField] protected GameObject reloadIndicator;
        [Required] [SerializeField] protected TextMeshProUGUI ammoText;
        

        public void UpdateInput(CharacterInput input)
        {
            if (input.Attack) Attack(input.AttackButtonDown, Character.Stats.FireRateModifier);
            if (input.Reload) Reload();
        }

        public override void Attack(bool pressedThisFrame, float fireRateMultipier)
        {
            base.Attack(pressedThisFrame, fireRateMultipier);
            
            if (Weapon is null)
            {
                ammoText.text = null;
                reloadIndicator.SetActive(false);
                return;
            }
            
            ammoText.text = $"{Weapon.Bullets} / {Weapon.ReserveAmmo}";
            reloadIndicator.SetActive(Weapon.Bullets <= Weapon.MagazineSize * 0.2);
        }

        protected override void Reload()
        {
            if (Weapon is null)
            {
                ammoText.text = null;
                reloadIndicator.SetActive(false);
                return;
            }
            
            base.Reload();
            ammoText.text = $"{Weapon.Bullets} / {Weapon.ReserveAmmo}";
            reloadIndicator.SetActive(Weapon.Bullets <= Weapon.MagazineSize * 0.2);
        }

        public override void ChangeWeapon(WeaponData weaponData)
        {
            base.ChangeWeapon(weaponData);
            
            if (Weapon is null) return;
            
            ammoText.text = $"{Weapon.Bullets} / {Weapon.ReserveAmmo}";
            reloadIndicator.SetActive(Weapon.Bullets <= Weapon.MagazineSize * 0.2);
        }
    }
}