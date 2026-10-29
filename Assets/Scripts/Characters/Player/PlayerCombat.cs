using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

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
            ammoText.text = $"{Weapon.Bullets} / {Weapon.MaxAmmo}";
            reloadIndicator.SetActive(Weapon.Bullets <= Weapon.MagazineSize * 0.2);
        }

        protected override void Reload()
        {
            base.Reload();
            ammoText.text = $"{Weapon.Bullets} / {Weapon.MaxAmmo}";
            reloadIndicator.SetActive(false);
        }

        protected override bool ChangeWeapon()
        {
            if (!base.ChangeWeapon()) return false;
            
            ammoText.text = $"{Weapon.Bullets} / {Weapon.MaxAmmo}";
            reloadIndicator.SetActive(Weapon.Bullets <= Weapon.MagazineSize * 0.2);
            return true;

        }
    }
}