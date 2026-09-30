using UnityEngine;

namespace Characters
{
    public abstract class ControllerBase : MonoBehaviour
    {        
        public abstract (RaycastHit?, Ray) GetRaycast(float distance);
    }
}
