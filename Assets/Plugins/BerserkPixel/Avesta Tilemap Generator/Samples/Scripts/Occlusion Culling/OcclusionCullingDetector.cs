using UnityEngine;

namespace BerserkPixel.Tilemap_Generator.OcclussionCulling
{
    public class OcclusionCullingDetector : MonoBehaviour
    {
        [SerializeField] private Transform startingPoint;
        [SerializeField] private float range;

        private void Awake()
        {
            var localPosition = startingPoint.localPosition;
            
            // initial detect
            DetectByDistance();
            
            localPosition.y = range;
            startingPoint.localPosition = localPosition;
        }

        private void Update()
        {
            DetectByDistance();
        }

        private void DetectByDistance()
        {
            RaycastHit2D hit = Physics2D.CircleCast(startingPoint.position, 1f, startingPoint.up);

            if (hit.collider != null && hit.transform.TryGetComponent(out IOcclusionCulling occlusionCulling))
            {
                if (occlusionCulling.OnShown())
                {
                    Debug.Log($"Distance Detected {hit.transform.name}");
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(startingPoint.position, 1f);
        }
    }
}