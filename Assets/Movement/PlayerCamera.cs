using UnityEngine;

namespace Movement
{
    public class PlayerCamera : MonoBehaviour
    {
        private Vector3 _eulerAngles;
        [SerializeField] private float cameraMaxRotationAngle;
        [SerializeField] private float cameraMinRotationAngle;
        [SerializeField] private float lookSensitivity = 0.3f;

        public void Initialize(Transform target)
        {
            transform.position = target.position;
            transform.eulerAngles = _eulerAngles = target.eulerAngles;
        }

        public void UpdateRotation(Vector2 direction)
        {
            _eulerAngles += new Vector3(-direction.y, direction.x) * lookSensitivity;
            if (_eulerAngles.x < cameraMaxRotationAngle && _eulerAngles.x > cameraMinRotationAngle)
            {
                transform.eulerAngles = _eulerAngles;
            }
            else
            {
                _eulerAngles -= new Vector3(-direction.y, direction.x) * lookSensitivity;
            }
        }

        public void UpdatePosition(Transform target)
        {
            transform.position = target.position;
        }
    }
}