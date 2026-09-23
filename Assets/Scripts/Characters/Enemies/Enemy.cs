using System;
using Characters.State_Machine;
using Game;
using UnityEngine;

namespace Characters.Enemies
{
    public abstract class Enemy : CharacterBase, IEntity
    {
        public EntitySpawner Spawner { get; protected set; }
        protected Detection _detection;
        
        [Header("Follow Settings")]
        [SerializeField] protected Transform followTarget;
        public Vector3 followOffset = Vector3.zero;
        public float followDistance = 0f;
        
        public Transform FollowTarget => followTarget;
        public bool HasDestination { get; protected set; }
        public bool HasArrived { get; protected set; }
        public Vector3 Destination { get; protected set; }
        public Transform CurrentTarget => _detection.CurrentTarget;
        public bool HasTarget => _detection.HasTarget;
        

        protected override void OnEnable()
        {
            base.OnEnable();

                
            _detection = GetComponent<Detection>();
            _detection.targetAcquired.AddListener(OnTargetAcquired);
            _detection.targetLost.AddListener(OnTargetLost);
        }

        protected virtual void OnDisable()
        {
            _detection.targetAcquired.RemoveListener(OnTargetAcquired);
            _detection.targetLost.RemoveListener(OnTargetLost);
        }




        public void SetSpawner(EntitySpawner spawner) => this.Spawner = spawner;

        public override void Damage(BodyPart bodyPart, int damage, float knockBack, Vector3 origin)
        {
            base.Damage(bodyPart, damage, knockBack, origin);
        }
        
        public void SetDestination(Vector3 pos)
        {
            followTarget = null;
            Destination = pos;
            HasDestination = true;
            HasArrived = false;
        }
        
        public void SetTarget(Transform target, float standoffDistance = 0f, Vector3? offset = null)
        {
            followTarget = target;
            followDistance = standoffDistance;
            followOffset = offset ?? Vector3.zero;
            HasDestination = target != null;
            HasArrived = false;
        }
 
        public void ClearTarget()
        {
            followTarget = null;
            HasDestination = false;
            HasArrived = false;
        }

        protected abstract void OnTargetAcquired(Transform target);

        protected abstract void OnTargetLost(Transform target);
    }
}