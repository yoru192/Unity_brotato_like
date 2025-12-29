using System;
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

            // Перевірка чи агент на NavMesh
            if (!_agent.isOnNavMesh)
            {
                Debug.Log($"{gameObject.name} is NOT on NavMesh! Position: {transform.position}");
                return;
            }

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
    }
}