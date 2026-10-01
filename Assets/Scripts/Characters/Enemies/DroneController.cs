using UnityEngine;

namespace Characters.Enemies
{
    public class DroneController : EnemyController
    {
        
        [SerializeField] private float arrivalRadius = 3f;
        [SerializeField] private float stoppingDistance = 0.5f;
        [SerializeField] private float altitudeDamping = 0.5f;
        [SerializeField] private float maxVerticalSpeed = 4f;
        [SerializeField] private float maxBankAngle = 25f;
        [SerializeField] private float bankSmoothing = 4f;
        [SerializeField] private float hoverHeight = 5f;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float altitudeCorrectionSpeed = 6f;

        private float _currentBank;
        private Rigidbody _rb;

        public override void Initialize(EnemyCharacter enemy)
        {
            base.Initialize(enemy);
            _rb = GetComponent<Rigidbody>();
            
            Vector3 initialFlatForward = new Vector3(transform.forward.x, 0f, transform.forward.z);
            if (initialFlatForward.sqrMagnitude < 0.001f) initialFlatForward = Vector3.forward;
            headingRotation = Quaternion.LookRotation(initialFlatForward.normalized, Vector3.up);
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
                        ? currentSpeed * (distance / arrivalRadius)
                        : currentSpeed;
                }
            }

            desiredDirection = ApplyObstacleAvoidance(desiredDirection);
            Vector3 horizontalDirection = new Vector3(desiredDirection.x, 0f, desiredDirection.z);

            Vector3 targetVelocity = desiredDirection * desiredSpeed;
            targetVelocity.y += CalculateVerticalVelocity();

            MoveTowards(targetVelocity);
            RotateAndBank(horizontalDirection);
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
        
        private float CalculateVerticalVelocity()
        {
            if (groundMask.value == 0) return 0f;

            if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit,
                    hoverHeight * 100f, groundMask))
                return 0f;

            float heightError = hoverHeight - hit.distance;
            if (Mathf.Abs(heightError) < 0.05f) heightError = 0f;
            float desiredVertical = heightError * altitudeCorrectionSpeed;

            desiredVertical -= _rb.linearVelocity.y * altitudeDamping;

            return Mathf.Clamp(desiredVertical, -maxVerticalSpeed, maxVerticalSpeed);
        }

        private void MoveTowards(Vector3 targetVelocity)
        {
            currentVelocity = Vector3.MoveTowards(
                _rb.linearVelocity, targetVelocity, character.Stats.Acceleration * Time.fixedDeltaTime);
            _rb.linearVelocity = currentVelocity;
        }

        private void RotateAndBank(Vector3 moveDirection)
        {
            Vector3 flatDirection = new Vector3(moveDirection.x, 0f, moveDirection.z);

            if (flatDirection.sqrMagnitude > 0.001f)
            {
                CalculateHeadingAndBank();
            }
            else if (HasTarget)
            {
                flatDirection = new Vector3(CurrentTarget.position.x - transform.position.x, 0f, CurrentTarget.position.z - transform.position.z);

                CalculateHeadingAndBank();
            }
            else
            {
                _currentBank = Mathf.Lerp(_currentBank, 0f, bankSmoothing * Time.fixedDeltaTime);
            }

            _rb.MoveRotation(headingRotation * Quaternion.Euler(0f, 0f, _currentBank));
            
            return;

            void CalculateHeadingAndBank()
            {
                flatDirection.Normalize();

                Quaternion targetHeading = Quaternion.LookRotation(flatDirection, Vector3.up);
                headingRotation = Quaternion.RotateTowards(
                    headingRotation, targetHeading, character.Stats.TurnSpeed * Time.fixedDeltaTime);


                Vector3 headingForward = headingRotation * Vector3.forward;
                float signedAngle = Vector3.SignedAngle(headingForward, flatDirection, Vector3.up);
                float targetBank = Mathf.Clamp(-signedAngle, -maxBankAngle, maxBankAngle);
                _currentBank = Mathf.Lerp(_currentBank, targetBank, bankSmoothing * Time.fixedDeltaTime);
            }
        }

        private void ApplyAltitudeHold(ref Vector3 direction)
        {
            if (groundMask.value == 0) return;

            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit,
                    hoverHeight * 100, groundMask))
            {
                float currentHeight = hit.distance;
                float heightError = hoverHeight - currentHeight;

                //TODO: Fix this math
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

        public override (RaycastHit?, Ray) GetRaycast(float distance)
        {
            throw new System.NotImplementedException();
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
                
                if (!(followDistance > 0.001f)) return;
                Gizmos.color = new Color(0f, 0f, 1f, 0.3f);
                Gizmos.DrawWireSphere(followTarget.position + followOffset, followDistance);
            }
        }
    }
}