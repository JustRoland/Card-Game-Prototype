using UnityEngine;

namespace Characters
{
    public abstract class ControllerBase : MonoBehaviour
    {
        [SerializeField] protected bool debug;
        public abstract (RaycastHit?, Ray) GetRaycast(float distance);
    }
}
