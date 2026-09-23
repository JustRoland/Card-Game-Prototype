using Characters.State_Machine;
using UnityEngine;

namespace Characters.Enemies
{
    public class Drone : Enemy
    {
        [SerializeField] private float arrivalRadius = 3f;
        [SerializeField] private float stoppingDistance = 0.5f;
        [SerializeField] private float maxBankAngle = 25f;
        [SerializeField] private float bankSmoothing = 4f;
        [SerializeField] private float hoverHeight = 5f;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float altitudeCorrectionSpeed = 6f;

        [Header("Obstacle Avoidance")] [SerializeField]
        private LayerMask obstacleMask;

        [SerializeField] private float avoidanceLookahead = 4f;
        [SerializeField] private float avoidanceRadius = 0.75f;
        [Range(0f, 1f)] [SerializeField] private float avoidanceWeight = 0.8f;
        

        public Vector3 Velocity => _rb.linearVelocity;
        private StateMachine<Drone> stateMachine;
        

        private Rigidbody _rb;
        private Vector3 _currentVelocity;
        private float _currentBank;
        private Quaternion _headingRotation;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            
            Vector3 initialFlatForward = new Vector3(transform.forward.x, 0f, transform.forward.z);
            if (initialFlatForward.sqrMagnitude < 0.001f) initialFlatForward = Vector3.forward;
            _headingRotation = Quaternion.LookRotation(initialFlatForward.normalized, Vector3.up);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
                        
            stateMachine = new StateMachine<Drone>(this);
            stateMachine.OnStateChanged += StateChangeDebugMessage;
            stateMachine.Start<DroneIdle>();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            stateMachine.OnStateChanged -= StateChangeDebugMessage;
        }
        
        protected void Update()
        {
            stateMachine.Update(Time.deltaTime);
        }
        
        private void StateChangeDebugMessage(State<Drone> from, State<Drone> to) => print($"[Enemy] {from?.Name ?? "null"} -> {to.Name}");
        
        protected override void OnTargetAcquired(Transform target)
        {
        }

        protected override void OnTargetLost(Transform target)
        {
        }

        private void FixedUpdate()
        {
            UpdateFollowTarget();
            
            Vector3 desiredDirection = Vector3.zero;
            float desiredSpeed = 0f;

            if (HasDestination)
            {
                Vector3 toDestination = Destination - transform.position;
                float distance = toDestination.magnitude;

                if (distance <= stoppingDistance)
                {
                    HasArrived = true;
                }
                else
                {
                    HasArrived = false;
                    desiredDirection = toDestination.normalized;

                    // Ease speed down inside the arrival radius
                    desiredSpeed = distance < arrivalRadius
                        ? Stats.WalkSpeed * (distance / arrivalRadius)
                        : Stats.WalkSpeed;
                }
            }

            desiredDirection = ApplyObstacleAvoidance(desiredDirection);
            ApplyAltitudeHold(ref desiredDirection);

            MoveTowards(desiredDirection, desiredSpeed);
            RotateAndBank(desiredDirection);
        }
        
        private void UpdateFollowTarget()
        {
            if (!followTarget) return;
 
            Vector3 followPoint = followTarget.position + followOffset;
 
            if (followDistance > 0.001f)
            {
                Vector3 awayFromTarget = transform.position - followPoint;
                if (awayFromTarget.sqrMagnitude > 0.001f)
                {
                    followPoint += awayFromTarget.normalized * followDistance;
                }
            }
 
            Destination = followPoint;
            HasDestination = true;
        }

        private void MoveTowards(Vector3 direction, float speed)
        {
            Vector3 targetVelocity = direction * speed;
            _currentVelocity = Vector3.MoveTowards(
                _rb.linearVelocity, targetVelocity, Stats.Acceleration * Time.fixedDeltaTime);
            _rb.linearVelocity = _currentVelocity;
        }

        private void RotateAndBank(Vector3 moveDirection)
        {
            Vector3 flatDirection = new Vector3(moveDirection.x, 0f, moveDirection.z);

            if (flatDirection.sqrMagnitude > 0.001f)
            {
                flatDirection.Normalize();
                
                Quaternion targetHeading = Quaternion.LookRotation(flatDirection, Vector3.up);
                _headingRotation = Quaternion.RotateTowards(
                    _headingRotation, targetHeading, Stats.TurnSpeed * Time.fixedDeltaTime);


                Vector3 headingForward = _headingRotation * Vector3.forward;
                float signedAngle = Vector3.SignedAngle(headingForward, flatDirection, Vector3.up);
                float targetBank = Mathf.Clamp(-signedAngle, -maxBankAngle, maxBankAngle);
                _currentBank = Mathf.Lerp(_currentBank, targetBank, bankSmoothing * Time.fixedDeltaTime);
            }
            else
            {
                _currentBank = Mathf.Lerp(_currentBank, 0f, bankSmoothing * Time.fixedDeltaTime);
            }
            
            transform.rotation = _headingRotation * Quaternion.Euler(0f, 0f, _currentBank);
        }

        private void ApplyAltitudeHold(ref Vector3 direction)
        {
            if (groundMask.value == 0) return;

            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit,
                    hoverHeight * 3f, groundMask))
            {
                float currentHeight = hit.distance;
                float heightError = hoverHeight - currentHeight;

                float verticalCorrection = Mathf.Clamp(
                    heightError * altitudeCorrectionSpeed * Time.fixedDeltaTime, -1f, 1f);
                direction += Vector3.up * verticalCorrection;
                direction.Normalize();
            }
        }

        private Vector3 ApplyObstacleAvoidance(Vector3 desiredDirection)
        {
            if (obstacleMask.value == 0 || desiredDirection.sqrMagnitude < 0.001f)
                return desiredDirection;

            bool blocked = Physics.SphereCast(
                transform.position, avoidanceRadius, desiredDirection,
                out RaycastHit hit, avoidanceLookahead, obstacleMask);

            if (!blocked) return desiredDirection;

            Vector3 avoidDirection = Vector3.Reflect(desiredDirection, hit.normal).normalized;
            Vector3 blended = Vector3.Slerp(desiredDirection, avoidDirection, avoidanceWeight);
            return blended.normalized;
        }

        public override void Damage(BodyPart bodyPart, int damage, float knockBack, Vector3 origin)
        {
            base.Damage(bodyPart, damage, knockBack, origin);
            
            if (Stats.Health > 0)
            {
                // TODO: Add Investigate state?
                stateMachine.ChangeState<DroneChase>();
                SetDestination(origin);
            }
            else
            {
                stateMachine.ChangeState<DroneDestroy>();
            }
            
            


        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, arrivalRadius);
 
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, transform.forward * avoidanceLookahead);
            Gizmos.DrawWireSphere(
                transform.position + transform.forward * avoidanceLookahead, avoidanceRadius);
 
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, Vector3.down * hoverHeight);
 
            if (HasDestination)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, Destination);
                Gizmos.DrawWireSphere(Destination, stoppingDistance);
            }
 
            if (followTarget != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(followTarget.position + followOffset, 0.3f);
                if (followDistance > 0.001f)
                {
                    Gizmos.color = new Color(0f, 0f, 1f, 0.3f);
                    Gizmos.DrawWireSphere(followTarget.position + followOffset, followDistance);
                }
            }
        }
    }
}