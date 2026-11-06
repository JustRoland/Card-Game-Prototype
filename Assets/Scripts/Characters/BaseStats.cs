using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters
{
    [CreateAssetMenu(fileName = "New Stats", menuName = "Base Stats")]
    public class BaseStats : ScriptableObject
    {
        [Title("Movement")]
        public float acceleration = 135;
        public float walkSpeed = 4.5f;
        public float crouchSpeed = 3;
        public float sprintSpeed = 10;
        public float slowedSpeed = 2;
        public float speedModifier = 1f;
        public float jumpForce = 8;
        public float airAcceleration = 2;
        public float dashForce = 40;
        public float dashAirDistance = 3;
        public float dashCooldown = 2;
        public float turnSpeed = 180;
        
        [HorizontalGroup("Health", Title = "Combat")] public float health = 100f;
        [HorizontalGroup("Health")] public float healthIncrease = 0f;
        [HorizontalGroup("DamageAdded")]public float damageAdded = 0f;
        [HorizontalGroup("DamageAdded")]public float dmgAddIncrease = 0f;
        [HorizontalGroup("DamageMultiplier")]public float damageMultiplier = 1f;
        [HorizontalGroup("DamageMultiplier")]public float dmgMultIncrease = 0f;
        public float maxHealth = 100f;
        public int revives = 0;
        public float defense = 10f;
        public float maxRange = 100f;
        public float fireRateModifier = 1f;
        public float reloadTimeModifier = 1f;
        public float recoil = 0.5f;
        public float weakenedDamageMultiplier = 1.1f;
        public float explosiveShotRadius = 0.4f;
    }
}