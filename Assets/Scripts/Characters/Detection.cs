using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Characters
{
    public class Detection : MonoBehaviour
    {
        [SerializeField] private Transform viewOrigin;
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float loseRange = 14f;
        [Range(0f, 360f)] [SerializeField] private float fieldOfViewAngle = 110f;
        [SerializeField] private LayerMask detectionMask;
        [SerializeField] private LayerMask obstructionMask;

        [SerializeField] private float scanInterval = 0.2f;
        [SerializeField] private int maxCandidates = 6;

        public bool HasTarget { get; private set; }
        public Transform CurrentTarget { get; private set; }

        public IReadOnlyList<Transform> VisibleTargets => _visibleTargets;

        public UnityEvent<Transform> targetAcquired;
        public UnityEvent<Transform> targetLost;

        private readonly List<Transform> _visibleTargets = new();
        private readonly Collider[] _candidateBuffer = new Collider[32];
        private float _scanTimer;


        private void Awake()
        {
            if (!viewOrigin) viewOrigin = transform;
            if (loseRange < detectionRange) loseRange = detectionRange;
        }

        private void Update()
        {
            _scanTimer -= Time.deltaTime;
            if (_scanTimer > 0f) return;
            _scanTimer = scanInterval;

            Scan();
        }

        private void Scan()
        {
            _visibleTargets.Clear();

            float scanRadius = Mathf.Max(detectionRange, loseRange);
            int count = Physics.OverlapSphereNonAlloc(
                viewOrigin.position, scanRadius, _candidateBuffer, detectionMask);

            count = Mathf.Min(count, maxCandidates);

            Transform bestCandidate = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                Transform candidate = _candidateBuffer[i].transform;
                if (candidate == transform) continue;
                float distance = Vector3.Distance(viewOrigin.position, candidate.position);


                bool isCurrent = candidate == CurrentTarget;
                float effectiveRange = isCurrent ? loseRange : detectionRange;
                if (distance > effectiveRange) continue;

                if (!IsWithinFieldOfView(candidate.position)) continue;
                if (!HasLineOfSight(candidate.position)) continue;

                _visibleTargets.Add(candidate);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestCandidate = candidate;
                }
            }

            _visibleTargets.Sort((a, b) =>
                Vector3.Distance(viewOrigin.position, a.position)
                    .CompareTo(Vector3.Distance(viewOrigin.position, b.position)));

            UpdateCurrentTarget(bestCandidate);
        }

        private void UpdateCurrentTarget(Transform candidate)
        {
            Transform previous = CurrentTarget;

            if (candidate)
            {
                CurrentTarget = candidate;
                HasTarget = true;
                if (previous != candidate) targetAcquired?.Invoke(candidate);
            }
            else
            {
                CurrentTarget = null;
                HasTarget = false;
                if (previous) targetLost?.Invoke(previous);
            }
        }

        private bool IsWithinFieldOfView(Vector3 targetPosition)
        {
            if (fieldOfViewAngle >= 360f) return true;

            Vector3 toTarget = (targetPosition - viewOrigin.position).normalized;
            float angle = Vector3.Angle(viewOrigin.forward, toTarget);
            return angle <= fieldOfViewAngle * 0.5f;
        }

        private bool HasLineOfSight(Vector3 targetPosition)
        {
            if (obstructionMask.value == 0) return true;

            Vector3 origin = viewOrigin.position;
            Vector3 direction = targetPosition - origin;
            float distance = direction.magnitude;

            return !Physics.Raycast(origin, direction.normalized, distance, obstructionMask);
        }
        
        

        private void OnDrawGizmosSelected()
        {
            Vector3 origin = viewOrigin != null ? viewOrigin.position : transform.position;
            Vector3 forward = viewOrigin != null ? viewOrigin.forward : transform.forward;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(origin, detectionRange);

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            Gizmos.DrawWireSphere(origin, loseRange);

            if (fieldOfViewAngle < 360f)
            {
                Gizmos.color = Color.cyan;
                Quaternion leftRot = Quaternion.AngleAxis(-fieldOfViewAngle * 0.5f, Vector3.up);
                Quaternion rightRot = Quaternion.AngleAxis(fieldOfViewAngle * 0.5f, Vector3.up);
                Gizmos.DrawRay(origin, leftRot * forward * detectionRange);
                Gizmos.DrawRay(origin, rightRot * forward * detectionRange);
            }

            Gizmos.color = Color.red;
            if (CurrentTarget != null) Gizmos.DrawLine(origin, CurrentTarget.position);
        }
    }
}