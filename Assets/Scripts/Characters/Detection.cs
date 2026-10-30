using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Characters
{
    public class Detection : MonoBehaviour
    {
        [FoldoutGroup("Detection Settings")]
        [SerializeField] private Transform viewOrigin;
        [FoldoutGroup("Detection Settings")][SerializeField] private float detectionRange = 10f;
        [FoldoutGroup("Detection Settings")][SerializeField] private float loseRange = 14f;
        [FoldoutGroup("Detection Settings")][Range(0f, 360f)] [SerializeField] private float fieldOfViewAngle = 110f;
        [FoldoutGroup("Detection Settings")][SerializeField] private LayerMask detectionMask;
        [FoldoutGroup("Detection Settings")][SerializeField] private LayerMask obstructionMask;

        [FoldoutGroup("Detection Settings")][SerializeField] private float scanInterval = 0.2f;
        [FoldoutGroup("Detection Settings")][SerializeField] private int maxCandidates = 6;

        [ShowInInspector][ReadOnly] public bool HasTarget => CurrentTarget;
        [ShowInInspector][ReadOnly] public Transform CurrentTarget { get; private set; }

        public IReadOnlyList<Transform> VisibleTargets => _visibleTargets;

        [FoldoutGroup("Events")] public UnityEvent<Transform> targetAcquired;
        [FoldoutGroup("Events")] public UnityEvent<Transform> targetLost;

        private readonly List<Transform> _visibleTargets = new();
        private readonly Collider[] _candidateBuffer = new Collider[32];
        private float _scanTimer;
        [SerializeField] private bool debug;
        [ShowInInspector][ReadOnly] private bool _isEnabled;


        public void Initialzie()
        {
            _isEnabled = true;
            if (!viewOrigin) viewOrigin = transform;
            if (loseRange < detectionRange) loseRange = detectionRange;
        }

        private void Update()
        {
            if (!_isEnabled) return;
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
                if (previous != candidate) targetAcquired?.Invoke(candidate);
            }
            else
            {
                CurrentTarget = null;
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
            if (!debug) return;
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