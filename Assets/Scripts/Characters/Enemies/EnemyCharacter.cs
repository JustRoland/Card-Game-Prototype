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
        private EnemyCharacter _reinforcementCaller;

        protected override void OnEnable()
        {
            base.OnEnable();
            controller = GetComponent<EnemyController>();
            combat = GetComponent<CombatBase>();
            controller?.Initialize(this);
            combat?.Initialize(controller);
        }
        protected virtual void OnDisable()
        {
            UnbindFromCaller();
        }
        
        public void BindToCaller(EnemyCharacter caller)
        {
            // Unbind any previous caller first
            UnbindFromCaller();

            _reinforcementCaller = caller;
            if (_reinforcementCaller)
            {
                _reinforcementCaller.targetLocationUpdate.AddListener(OnCallerTargetLocationUpdate);
            }
        }

        private void OnCallerTargetLocationUpdate(Vector3 destination)
        {
            SetDestination(destination, true);
        }

        private void UnbindFromCaller()
        {
            if (!_reinforcementCaller) return;
            _reinforcementCaller.targetLocationUpdate.RemoveListener(OnCallerTargetLocationUpdate);
            _reinforcementCaller = null;
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