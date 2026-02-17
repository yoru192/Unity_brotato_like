using System.Linq;
using CodeBase.Logic;
using CodeBase.Player;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class EnemyAttack : MonoBehaviour
    {
        private EnemyAnimator _animator;
        private float _attackCooldown;
        private float _radius;
        private int _damage;
        private Transform _playerTransform;
        private float _currentAttackCooldown;
        private bool _isAttacking;
        private Collider2D[] _hits = new Collider2D[1];
        private int _layerMask;
        private bool _attackIsActive;
        private Collider2D _enemyCollider;

        public void Construct(Transform playerTransform, EnemyAnimator animator, float attackCooldown,
            float radius, int damage)
        {
            _playerTransform = playerTransform;
            _animator = animator;
            _attackCooldown = attackCooldown;
            _radius = radius;
            _damage = damage;
            _currentAttackCooldown = attackCooldown;
        }

        private void Awake()
        {
            _layerMask = 1 << LayerMask.NameToLayer("Player");
            _enemyCollider = GetComponent<Collider2D>();
        }

        private void Update()
        {
            UpdateCooldown();

            if (CanAttack())
                StartAttack();
        }

        private void UpdateCooldown()
        {
            if (!CooldownIsUp())
                _currentAttackCooldown -= Time.deltaTime;
        }

        private void OnAttack()
        {
            if (Hit(out Collider2D hit))
            {
                hit.GetComponentInParent<PlayerHealth>().TakeDamage(_damage);
            }
        }

        private void OnAttackEnded()
        {
            _currentAttackCooldown = _attackCooldown;
            _isAttacking = false;
        }

        private bool Hit(out Collider2D hit)
        {
            var hitAmount = Physics2D.OverlapCircleNonAlloc(
                transform.position, 
                _radius,
                _hits, 
                _layerMask);
            
            hit = _hits.FirstOrDefault();
            return hitAmount > 0;
        }

        private bool CooldownIsUp()
        {
            return _currentAttackCooldown <= 0f;
        }

        private bool CanAttack()
        {
            if (!_isAttacking && CooldownIsUp())
            {
                if (_playerTransform.TryGetComponent<Collider2D>(out var playerCollider))
                {
                    ColliderDistance2D distanceInfo = Physics2D.Distance(_enemyCollider, playerCollider);
                    return distanceInfo.distance <= _radius;
                }
                else
                {
                    float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);
                    return distanceToPlayer <= _radius;
                }
            }
            else
            {
                return false;
            }
        }

        private void StartAttack()
        {
            _animator.PlayAttack();
            _isAttacking = true;
        }
        
        private void OnDrawGizmosSelected()
        {
            Collider2D enemyCol = _enemyCollider != null ? _enemyCollider : GetComponent<Collider2D>();
            
            // Радіус атаки
            Gizmos.color = Color.red;
            DrawCircle(transform.position, _radius, 30);
            
            // Показуємо колайдер ворога
            if (enemyCol != null && enemyCol is CircleCollider2D circle)
            {
                Gizmos.color = Color.cyan;
                DrawCircle(transform.position, circle.radius, 20);
            }
            
            if (_playerTransform != null)
            {
                Collider2D playerCol = _playerTransform.GetComponent<Collider2D>();
                float distance = 0;
                
                if (enemyCol != null && playerCol != null)
                {
                    // Реальна дистанція між колайдерами
                    ColliderDistance2D distanceInfo = Physics2D.Distance(enemyCol, playerCol);
                    distance = distanceInfo.distance;
                    
                    // Показуємо найближчу точку між колайдерами
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(distanceInfo.pointA, distanceInfo.pointB);
                    Gizmos.DrawWireSphere(distanceInfo.pointA, 0.1f);
                    Gizmos.DrawWireSphere(distanceInfo.pointB, 0.1f);
                }
                else
                {
                    distance = Vector2.Distance(transform.position, _playerTransform.position);
                }
                
                // Зелена = в радіусі, жовта = поза радіусом
                Gizmos.color = distance <= _radius ? Color.green : Color.yellow;
                Gizmos.DrawLine(transform.position, _playerTransform.position);
                
                #if UNITY_EDITOR
                Vector3 midPoint = (transform.position + _playerTransform.position) / 2f;
                UnityEditor.Handles.Label(midPoint, $"Collider Dist: {distance:F2}");
                #endif
            }
            
            if (_isAttacking)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, _radius * 1.1f);
            }
            
            if (!CooldownIsUp())
            {
                Gizmos.color = Color.cyan;
                float cooldownPercent = _currentAttackCooldown / _attackCooldown;
                Gizmos.DrawWireSphere(transform.position, _radius * cooldownPercent);
            }
        }

        private void DrawCircle(Vector3 center, float radius, int segments)
        {
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }
    }
}