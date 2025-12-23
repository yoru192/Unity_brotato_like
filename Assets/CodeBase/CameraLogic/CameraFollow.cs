using UnityEngine;

namespace CodeBase.CameraLogic
{
    public class CameraFollow : MonoBehaviour 
    {
        [SerializeField]
        public Transform target;
        public Vector3 offset = new Vector3(0, 0, -10);
        
        
        void LateUpdate() 
        {
            if (target is null) return;
        
            Vector3 desiredPosition = target.position + offset;
            transform.position = desiredPosition;
        }
        public void Follow(GameObject following)
        {
            target = following.transform;
        }
    }

}