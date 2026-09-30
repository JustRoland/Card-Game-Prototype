using System;
using Characters.State_Machine;
using Game;
using UnityEngine;
using UnityEngine.Events;

namespace Characters.Enemies
{
    public abstract class EnemyCharacter : CharacterBase, IEntity
    {
        protected CombatBase combat;
        protected EnemyController controller;
        public EnemySpawner Spawner { get; private set; }
        public bool HasDestination => controller.HasDestination;
        public bool HasArrived => controller.HasArrived;
        public Vector3 Destination => controller.Destination;
        public Transform CurrentTarget => controller.CurrentTarget;
        public bool HasTarget => controller.HasTarget;
        public bool HasWeapon => combat;
        public float MaxRange => combat.MaxRange;

        public UnityEvent<Vector3> targetLocationUpdate = new();

        protected override void OnEnable()
        {
            base.OnEnable();
            controller = GetComponent<EnemyController>();
            combat = GetComponent<CombatBase>();
            controller?.Initialize(this);
            combat?.Initialize(controller);
        }
        

        public void SetSpawner(EnemySpawner spawner) => this.Spawner = spawner;

        public override void Damage(BodyPart bodyPart, int damage, float knockBack, Vector3 origin)
        {
            base.Damage(bodyPart, damage, knockBack, origin);
        }

        public void Attack() => combat?.Attack(true);
        public virtual void SetDestination(Vector3 destination, bool investigate) => controller.SetDestination(destination);
        public void SetTarget(Transform target) => controller.SetTarget(target);
        public void ClearTarget() => controller.ClearTarget();
        public void SetSpeed(float speed) => controller.currentSpeed = speed;
    }
}