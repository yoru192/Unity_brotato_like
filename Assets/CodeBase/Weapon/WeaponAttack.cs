using System.Linq;
using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Weapon
{
    public class WeaponAttack : MonoBehaviour
    {
        private float _attackCooldown;
        private float _effectiveDistance;
        private float _radius;
        private int _damage;

        private WeaponAnimator _animator;
        private float _currentCooldown;
        private int _layerMask;
        private bool _isAttacking;
        private Collider2D[] _hits = new Collider2D[1];
        private Collider2D _currentTarget;

        public void Construct(WeaponAnimator animator, float effectiveDistance, float radius, int damage, float cooldown)
        {
            _animator = animator;
            _effectiveDistance = effectiveDistance;
            _radius = radius;
            _damage = damage;
            _attackCooldown = cooldown;
        }

        private void Awake()
        {
            _layerMask = 1 << LayerMask.NameToLayer("Hittable");
        }

        private void Update()
        {
            UpdateCooldown();

            if (CanAttack() && HasTarget(out _currentTarget))
                StartAttack();
        }

        public void OnAttack()
        {
            if (_currentTarget != null && _currentTarget.gameObject.activeInHierarchy)
            {
                var health = _currentTarget.GetComponent<IHealth>();
                if (health != null)
                {
                    Debug.Log($"Hit enemy: {_currentTarget.name}");
                    health.TakeDamage(_damage);
                }
            }
        }

        public void OnAttackEnded()
        {
            _currentCooldown = _attackCooldown;
            _isAttacking = false;
            _currentTarget = null;
        }

        private void UpdateCooldown()
        {
            if (!CooldownIsUp())
                _currentCooldown -= Time.deltaTime;
        }

        private bool CooldownIsUp() => _currentCooldown <= 0f;

        private bool CanAttack() => !_isAttacking && CooldownIsUp();

        private bool HasTarget(out Collider2D target)
        {
            int hitAmount = Physics2D.OverlapCircleNonAlloc(StartPoint(), _radius, _hits, _layerMask);
            target = _hits.FirstOrDefault();
            return hitAmount > 0;
        }

        private Vector2 StartPoint()
        {
            return (Vector2)transform.position + Vector2.up * 0.5f + (Vector2)transform.right * _effectiveDistance;
        }

        private void StartAttack()
        {
            _animator.PlayAttack();
            _isAttacking = true;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            Gizmos.color = _isAttacking ? Color.yellow : Color.red;
            
            Vector3 center = StartPoint();
            float angle = 0f;
            Vector3 lastPoint = center + new Vector3(_radius, 0, 0);
            
            for (int i = 1; i <= 32; i++)
            {
                angle = i * (360f / 32f) * Mathf.Deg2Rad;
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * _radius, Mathf.Sin(angle) * _radius, 0);
                Gizmos.DrawLine(lastPoint, newPoint);
                lastPoint = newPoint;
            }
        }
    }
}
