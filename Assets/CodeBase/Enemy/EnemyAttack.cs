using System.Linq;
using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class EnemyAttack : MonoBehaviour
    {
        private EnemyAnimator _animator;
        private float _attackCooldown;
        private float _radius;
        private float _effectiveDistance;
        private int _damage;

        private Transform _playerTransform;
        private float _currentAttackCooldown;
        private bool _isAttacking;
        private Collider2D[] _hits = new Collider2D[1];
        private int _layerMask;

        public void Construct(Transform playerTransform, EnemyAnimator animator, float attackCooldown, float radius,
            float effectiveDistance, int damage)
        {
            _playerTransform = playerTransform;
            _animator = animator;
            _attackCooldown = attackCooldown;
            _radius = radius;
            _effectiveDistance = effectiveDistance;
            _damage = damage;
        }

        private void Awake()
        {
            _layerMask = 1 << LayerMask.NameToLayer("Player");
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
                hit.GetComponent<IHealth>().TakeDamage(_damage);
            }
        }

        private void OnAttackEnded()
        {
            _currentAttackCooldown = _attackCooldown;
            _isAttacking = false;
        }

        private bool Hit(out Collider2D hit)
        {
            var hitAmount = Physics2D.OverlapCircleNonAlloc(StartPoint(), _radius, _hits, _layerMask);
            hit = _hits.FirstOrDefault();
            return hitAmount > 0;
        }

        private bool CooldownIsUp()
        {
            return _currentAttackCooldown <= 0f;
        }

        private Vector2 StartPoint()
        {
            return (Vector2)transform.position + (Vector2)transform.right * _effectiveDistance;
        }

        private bool CanAttack()
        {
            if (_isAttacking || !CooldownIsUp() || _playerTransform == null)
                return false;

            float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);
            return distanceToPlayer <= _effectiveDistance + _radius;
        }

        private void StartAttack()
        {
            _animator.PlayAttack();
            _isAttacking = true;
        }
    }
}
