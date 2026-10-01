using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace Characters.Enemies
{

    [RequireComponent(typeof(NavMeshAgent))]
    public class DummyController : EnemyController
    {
        [SerializeField] private float arrivalRadius = 3f;
        [SerializeField] private float stoppingDistance = 0.5f;


        [SerializeField] private float repathInterval = 0.2f;
        [SerializeField] private float repathDistance = 0.25f;
        [SerializeField] private float navMeshSampleDistance = 2f;

        [Header("Ground Alignment")] [SerializeField]
        private LayerMask groundMask;

        [SerializeField] private float groundCheckDistance = 1.5f;
        [SerializeField] private bool alignToSlope = true;
        [SerializeField] private float slopeAlignSpeed = 8f;
        [SerializeField] private float maxSlopeAngle = 45f;

        [ShowInInspector] private bool _isGrounded;

        private NavMeshAgent _agent;
        private Vector3 _lastRequestedDestination;
        private float _nextRepathTime;
        private bool _hasRequestedPath;

        public override void Initialize(EnemyCharacter enemy)
        {
            base.Initialize(enemy);

            Vector3 initialFlatForward = new Vector3(transform.forward.x, 0f, transform.forward.z);
            if (initialFlatForward.sqrMagnitude < 0.001f) initialFlatForward = Vector3.forward;
            headingRotation = Quaternion.LookRotation(initialFlatForward.normalized, Vector3.up);

            SetupAgent();
        }

        private void SetupAgent()
        {
            _agent = GetComponent<NavMeshAgent>();

            _agent.updatePosition = true;
            _agent.updateRotation = false; // heading + slope alignment handled below
            _agent.autoBraking = true;
            _agent.stoppingDistance = stoppingDistance;
            _agent.speed = currentSpeed;
            _agent.acceleration = character.Stats.Acceleration;

            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
                _agent.Warp(hit.position);
            else
                Debug.LogWarning($"{name}: no NavMesh found near spawn position.", this);
        }

        public override void SetDestination(Vector3 destination)
        {
            if (!_agent.isOnNavMesh || !gameObject.activeSelf) return;
            base.SetDestination(destination);
            _agent.SetDestination(destination);
            _agent.speed = currentSpeed;
            _lastRequestedDestination = destination;
            _hasRequestedPath = true;
        }

        private void Update()
        {
            if (!_agent || !_agent.isOnNavMesh)
                return;

            UpdateFollowTarget();
            CheckGrounded(out Vector3 groundNormal);

            UpdatePath();
            UpdateSpeedAndArrival();
            
            RotateTowardsMovement(_agent.desiredVelocity);
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

        private void UpdatePath()
        {
            if (!HasDestination)
            {
                if (_hasRequestedPath)
                {
                    _agent.ResetPath();
                    _hasRequestedPath = false;
                }
                
                return;
            }

            bool movedEnough = !_hasRequestedPath ||
                               (Destination - _lastRequestedDestination).sqrMagnitude >
                               repathDistance * repathDistance;

            if (!movedEnough || Time.time < _nextRepathTime)
                return;

            _nextRepathTime = Time.time + repathInterval;

            // Snap the destination onto the NavMesh so off-mesh points don't fail outright.
            if (NavMesh.SamplePosition(Destination, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                _agent.SetDestination(hit.position);
                _lastRequestedDestination = Destination;
                _hasRequestedPath = true;
            }
        }

        private void UpdateSpeedAndArrival()
        {
            if (!HasDestination || !_hasRequestedPath || _agent.pathPending)
            {
                if (!HasDestination) _agent.speed = currentSpeed;
                return;
            }

            if (_agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                HasArrived = false;
                return;
            }

            float remaining = _agent.remainingDistance;
            HasArrived = remaining <= stoppingDistance + 0.05f;
            
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

        private void RotateTowardsMovement(Vector3 moveDirection)
        {
            Vector3 flatDirection = new Vector3(moveDirection.x, 0f, moveDirection.z);

            if (flatDirection.sqrMagnitude > 0.001f)
            {
                SetRotation();
            }
            else if (HasTarget)
            {
                flatDirection = new Vector3(CurrentTarget.position.x - transform.position.x, 0f, CurrentTarget.position.z - transform.position.z);

                SetRotation();
            }

            return;

            void SetRotation()
            {
                if (flatDirection.sqrMagnitude < 0.0001f) return;
                flatDirection.Normalize();

                Quaternion targetHeading = Quaternion.LookRotation(flatDirection, Vector3.up);
                headingRotation = Quaternion.RotateTowards(
                    headingRotation, targetHeading, character.Stats.TurnSpeed * Time.deltaTime);
            }
        }

        private void AlignToGround(Vector3 groundNormal)
        {
            if (!alignToSlope || !_isGrounded)
            {
                transform.rotation = headingRotation;
                return;
            }

            float slopeAngle = Vector3.Angle(Vector3.up, groundNormal);
            Vector3 useNormal = slopeAngle <= maxSlopeAngle ? groundNormal : Vector3.up;

            Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, useNormal) * headingRotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, slopeRotation, slopeAlignSpeed * Time.deltaTime);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, arrivalRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position + Vector3.up * 0.1f, Vector3.down * groundCheckDistance);

            if (HasDestination)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(Destination, stoppingDistance);
            }

            if (Application.isPlaying && _agent != null && _agent.hasPath)
            {
                Vector3[] corners = _agent.path.corners;
                Gizmos.color = Color.cyan;
                for (int i = 0; i < corners.Length - 1; i++)
                    Gizmos.DrawLine(corners[i], corners[i + 1]);

                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(_agent.steeringTarget, 0.15f);
            }

            if (followTarget != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(followTarget.position + followOffset, 0.3f);

                if (!(followDistance > 0.001f)) return;
                Gizmos.color = new Color(0f, 0f, 1f, 0.3f);
                Gizmos.DrawWireSphere(followTarget.position + followOffset, followDistance);
            }
        }

        public override (RaycastHit?, Ray) GetRaycast(float distance)
        {
            throw new System.NotImplementedException();
        }
    }
}