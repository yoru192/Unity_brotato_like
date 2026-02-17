using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    public class AgentMoveToPlayer : MonoBehaviour
    {
        private const float MinimalDistance = 1f;
        
        private NavMeshAgent _agent;
        private Transform _heroTransform;

        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        private void Awake()
        {
            if (_agent != null)
            {
                _agent.updateRotation = false;
                _agent.updateUpAxis = false;
            }
        }

        public void Construct(Transform heroTransform)
        {
            _heroTransform = heroTransform;
        }

        private void Update()
        {
            if (_heroTransform == null || _agent == null)
                return;
        
            if (!_agent.isOnNavMesh)
            {
                return;
            }
            
            FlipTowardsTarget();
    
            float distance = Vector2.Distance(transform.position, _heroTransform.position);
            if (distance >= MinimalDistance)
            {
                _agent.SetDestination(_heroTransform.position);
            }
            else
            {
                _agent.ResetPath();
            }
        }
        
        private void FlipTowardsTarget()
        {
            if (_heroTransform == null) return;
    
            float direction = _heroTransform.position.x - transform.position.x;
    
            if (direction > 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else if (direction < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
        
    }
}