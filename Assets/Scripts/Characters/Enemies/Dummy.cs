using Characters.State_Machine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.Enemies
{
    public class Dummy : Enemy
    {
        [Header("State")]
        private StateMachine<Dummy> stateMachine;
        [ShowInInspector] private string CurrentState => stateMachine != null ? stateMachine.CurrentState.Name:"No state";
        
        [SerializeField] private float arrivalRadius = 3f;
        [SerializeField] private float stoppingDistance = 0.5f;

        [Header("Ground Alignment")] [SerializeField]
        private LayerMask groundMask;

        [SerializeField] private float groundCheckDistance = 1.5f;
        [SerializeField] private bool alignToSlope = true;
        [SerializeField] private float slopeAlignSpeed = 8f;
        [SerializeField] private float maxSlopeAngle = 45f;

        [Header("Obstacle Avoidance")] [SerializeField]
        private LayerMask obstacleMask;

        [SerializeField] private float avoidanceLookahead = 2f;
        [SerializeField] private float avoidanceRadius = 0.5f;
        [Range(0f, 1f)] [SerializeField] private float avoidanceWeight = 0.8f;

        public Vector3 Velocity => _rb.linearVelocity;


        private Rigidbody _rb;
        private Vector3 _currentVelocity;
        private Quaternion _headingRotation;
        [ShowInInspector] private bool _isGrounded;

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

            stateMachine = new StateMachine<Dummy>(this);
            stateMachine.OnStateChanged += StateChangeDebugMessage;
            stateMachine.Start<DummyIdle>();
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

        private void StateChangeDebugMessage(State<Dummy> from, State<Dummy> to) =>
            print($"[Enemy] {from?.Name ?? "null"} -> {to.Name}");

        protected override void OnTargetAcquired(Transform target)
        {
        }

        protected override void OnTargetLost(Transform target)
        {
        }

        public override void SetDestination(Vector3 destination, bool investigate)
        {
            base.SetDestination(destination, investigate);
            if (!investigate) return;
            if (stateMachine.CurrentState is DummyIdle or DummyPatrol) stateMachine.ChangeState<DummyInvestigate>();
        }

        private void FixedUpdate()
        {
            UpdateFollowTarget();
            CheckGrounded(out Vector3 groundNormal);

            Vector3 desiredDirection = Vector3.zero;
            float desiredSpeed = 0f;

            if (HasDestination)
            {
                Vector3 toDestination = Destination - transform.position;
                toDestination.y = 0f;
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
                        ? currentSpeed * (distance / arrivalRadius)
                        : currentSpeed;
                }
            }

            desiredDirection = ApplyObstacleAvoidance(desiredDirection);

            MoveTowards(desiredDirection, desiredSpeed);
            RotateTowardsMovement(desiredDirection);
            AlignToGround(groundNormal);
        }

        private void UpdateFollowTarget()
        {
            if (!followTarget) return;

            Vector3 followPoint = followTarget.position + followOffset;

            if (followDistance > 0.001f)
            {
                Vector3 awayFromTarget = transform.position - followPoint;
                awayFromTarget.y = 0f;
                if (awayFromTarget.sqrMagnitude > 0.001f)
                {
                    followPoint += awayFromTarget.normalized * followDistance;
                }
            }

            Destination = followPoint;
            HasDestination = true;
        }

        private void CheckGrounded(out Vector3 groundNormal)
        {
            groundNormal = Vector3.up;
 
            if (groundMask.value == 0)
            {
                _isGrounded = true;
                return;
            }
            
            if (Physics.Raycast(transform.position, Vector3.down,
                    out RaycastHit hit, groundCheckDistance, groundMask))
            {
                _isGrounded = true;
                groundNormal = hit.normal;
            }
            else
            {
                _isGrounded = false;
            }
        }
 
        private void MoveTowards(Vector3 direction, float speed)
        {
            Vector3 targetVelocity = direction * speed;
            targetVelocity.y = _rb.linearVelocity.y;
 
            _currentVelocity = Vector3.MoveTowards(
                _rb.linearVelocity, targetVelocity, Stats.Acceleration * Time.fixedDeltaTime);
            _rb.linearVelocity = _currentVelocity;
        }
 
        private void RotateTowardsMovement(Vector3 moveDirection)
        {
            Vector3 flatDirection = new Vector3(moveDirection.x, 0f, moveDirection.z);
 
            if (flatDirection.sqrMagnitude > 0.001f)
            {
                flatDirection.Normalize();
 
                Quaternion targetHeading = Quaternion.LookRotation(flatDirection, Vector3.up);
                _headingRotation = Quaternion.RotateTowards(
                    _headingRotation, targetHeading, Stats.TurnSpeed * Time.fixedDeltaTime);
            }
        }
 
        private void AlignToGround(Vector3 groundNormal)
        {
            if (!alignToSlope || !_isGrounded)
            {
                transform.rotation = _headingRotation;
                return;
            }
 
            float slopeAngle = Vector3.Angle(Vector3.up, groundNormal);
            Vector3 useNormal = slopeAngle <= maxSlopeAngle ? groundNormal : Vector3.up;
 
            Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, useNormal) * _headingRotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, slopeRotation, slopeAlignSpeed * Time.fixedDeltaTime);
        }
 
        private Vector3 ApplyObstacleAvoidance(Vector3 desiredDirection)
        {
            if (obstacleMask.value == 0 || desiredDirection.sqrMagnitude < 0.001f)
                return desiredDirection;
 
            Vector3 origin = transform.position + Vector3.up * 0.5f;
 
            bool blocked = Physics.SphereCast(
                origin, avoidanceRadius, desiredDirection,
                out RaycastHit hit, avoidanceLookahead, obstacleMask);
 
            if (!blocked) return desiredDirection;
 
            Vector3 avoidDirection = Vector3.Reflect(desiredDirection, hit.normal);
            avoidDirection.y = 0f;
            avoidDirection.Normalize();
 
            Vector3 blended = Vector3.Slerp(desiredDirection, avoidDirection, avoidanceWeight);
            return blended.normalized;
        }

        public override void Damage(BodyPart bodyPart, int damage, float knockBack, Vector3 origin)
        {
            base.Damage(bodyPart, damage, knockBack, origin);

            if (Stats.Health > 0)
            {
                SetDestination(origin, true);
            }
            else
            {
                stateMachine.ChangeState<DummyDestroy>();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, arrivalRadius);

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * avoidanceLookahead);
            Gizmos.DrawWireSphere(
                transform.position + Vector3.up * 0.5f + transform.forward * avoidanceLookahead, avoidanceRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position + Vector3.up * 0.1f, Vector3.down * groundCheckDistance);

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