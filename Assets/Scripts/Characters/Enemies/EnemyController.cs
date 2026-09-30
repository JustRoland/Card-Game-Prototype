using UnityEngine;
using UnityEngine.Events;

namespace Characters.Enemies
{
    public class EnemyController : ControllerBase
    {
        private Detection detection;
        public bool HasDestination { get; protected set; }
        public bool HasArrived { get; protected set; }
        public Vector3 Destination { get; protected set; }
        public Transform CurrentTarget => detection.CurrentTarget;
        public bool HasTarget => detection.HasTarget;
        
        [Header("Follow Settings")]
        public Transform followTarget;
        public Vector3 followOffset = Vector3.zero;
        public float followDistance = 0f;
        public float currentSpeed;
        
        [Header("Obstacle Avoidance")] [SerializeField]
        protected LayerMask obstacleMask;
        [SerializeField] protected float avoidanceLookahead = 2f;
        [SerializeField] protected float avoidanceRadius = 0.5f;
        [Range(0f, 1f)] [SerializeField] protected float avoidanceWeight = 0.8f;
        
        protected EnemyCharacter character;
        protected Rigidbody rb;
        protected Vector3 currentVelocity;
        protected Quaternion headingRotation;
        
        public virtual void Initialize(EnemyCharacter enemy)
        {
            character = enemy;
            detection = GetComponent<Detection>();
            rb = GetComponent<Rigidbody>();
        }


        
        public void SetDestination(Vector3 destination)
        {
            followTarget = null;
            Destination = destination;
            HasDestination = true;
            HasArrived = false;
        }
        
        public void SetTarget(Transform target, float standoffDistance = 0f, Vector3? offset = null)
        {
            followTarget = target;
            followDistance = standoffDistance > 0f ? standoffDistance : followDistance;
            followOffset = offset ?? followOffset;
            HasDestination = target;
            HasArrived = false;
        }
 
        public void ClearTarget()
        {
            followTarget = null;
            HasDestination = false;
            HasArrived = false;
        }
        
        public override (RaycastHit?, Ray) GetRaycast(float distance)
        {
            var ray = new Ray(transform.position, transform.forward);
            
            return Physics.Raycast(ray, out RaycastHit hit, distance) ? (hit, ray) : (null, ray);
            
        }
    }
}
