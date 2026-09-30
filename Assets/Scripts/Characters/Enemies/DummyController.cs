using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.Enemies
{
    public class DummyController : EnemyController
    {
        [SerializeField] private float arrivalRadius = 3f;
        [SerializeField] private float stoppingDistance = 0.5f;

        [Header("Ground Alignment")] [SerializeField]
        private LayerMask groundMask;

        [SerializeField] private float groundCheckDistance = 1.5f;
        [SerializeField] private bool alignToSlope = true;
        [SerializeField] private float slopeAlignSpeed = 8f;
        [SerializeField] private float maxSlopeAngle = 45f;
        
        [ShowInInspector] private bool _isGrounded;
        
        
        public override void Initialize(EnemyCharacter enemy)
        {
            base.Initialize(enemy);
            
            Vector3 initialFlatForward = new Vector3(transform.forward.x, 0f, transform.forward.z);
            if (initialFlatForward.sqrMagnitude < 0.001f) initialFlatForward = Vector3.forward;
            headingRotation = Quaternion.LookRotation(initialFlatForward.normalized, Vector3.up);
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
            targetVelocity.y = rb.linearVelocity.y;
 
            currentVelocity = Vector3.MoveTowards(
                rb.linearVelocity, targetVelocity, character.Stats.Acceleration * Time.fixedDeltaTime);
            rb.linearVelocity = currentVelocity;
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
                flatDirection.Normalize();
 
                Quaternion targetHeading = Quaternion.LookRotation(flatDirection, Vector3.up);
                headingRotation = Quaternion.RotateTowards(
                    headingRotation, targetHeading, character.Stats.TurnSpeed * Time.fixedDeltaTime);
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
