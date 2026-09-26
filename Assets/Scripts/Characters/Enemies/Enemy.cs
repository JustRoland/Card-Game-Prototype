using System;
using Characters.State_Machine;
using Game;
using UnityEngine;
using UnityEngine.Events;

namespace Characters.Enemies
{
    public abstract class Enemy : CharacterBase, IEntity
    {
        public EnemySpawner Spawner { get; private set; }
        private Detection detection;

        [Header("Follow Settings")]
        [SerializeField] protected Transform followTarget;
        public Vector3 followOffset = Vector3.zero;
        public float followDistance = 0f;
        
        public Transform FollowTarget => followTarget;
        public bool HasDestination { get; protected set; }
        public bool HasArrived { get; protected set; }
        public Vector3 Destination { get; protected set; }
        public Transform CurrentTarget => detection.CurrentTarget;
        public bool HasTarget => detection.HasTarget;
        protected float currentSpeed;
        
                
        public UnityEvent<Vector3> targetLocationUpdate = new();

        protected override void OnEnable()
        {
            base.OnEnable();

                
            detection = GetComponent<Detection>();
            detection?.targetAcquired.AddListener(OnTargetAcquired);
            detection?.targetLost.AddListener(OnTargetLost);
        }

        protected virtual void OnDisable()
        {
            detection?.targetAcquired.RemoveListener(OnTargetAcquired);
            detection?.targetLost.RemoveListener(OnTargetLost);
        }




        public void SetSpawner(EnemySpawner spawner) => this.Spawner = spawner;

        public override void Damage(BodyPart bodyPart, int damage, float knockBack, Vector3 origin)
        {
            base.Damage(bodyPart, damage, knockBack, origin);
        }
        
        public virtual void SetDestination(Vector3 destination, bool investigate)
        {
            followTarget = null;
            Destination = destination;
            HasDestination = true;
            HasArrived = false;
        }
        
        public virtual void SetTarget(Transform target, float standoffDistance = 0f, Vector3? offset = null)
        {
            followTarget = target;
            followDistance = standoffDistance;
            followOffset = offset ?? Vector3.zero;
            HasDestination = target;
            HasArrived = false;
        }
 
        public virtual void ClearTarget()
        {
            followTarget = null;
            HasDestination = false;
            HasArrived = false;
        }

        public void SetSpeed(float speed) => currentSpeed = speed;

        protected abstract void OnTargetAcquired(Transform target);

        protected abstract void OnTargetLost(Transform target);
    }
}