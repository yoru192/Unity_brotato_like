using System.Linq;
using UnityEngine;

namespace CodeBase.Weapon
{
    public class WeaponAttack : MonoBehaviour
    {
        [SerializeField] private float _attackCooldown;
        [SerializeField] private float _effectiveDistance;
        [SerializeField] private float _radius;
        [SerializeField] private int _damage = 10;
        
        private WeaponAnimator _animator;
        private float _currentCooldown;
        private int _layerMask;
        private bool _isAttacking;
        private Collider2D[] _hits = new Collider2D[1];

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
            
            if (CanAttack() && HasTarget()) // Тепер атакує тільки якщо є ціль
                StartAttack();
        }

        public void OnAttack()
        {
            if (Hit(out Collider2D hit))
            {
                Debug.Log($"Hit enemy: {hit.name}");
                // hit.GetComponent<IHealth>().TakeDamage(_damage);
            }
        }

        public void OnAttackEnded()
        {
            Debug.Log("Attack Ended"); // Для дебагу
            _currentCooldown = _attackCooldown;
            _isAttacking = false;
        }

        private void UpdateCooldown()
        {
            if (!CooldownIsUp())
                _currentCooldown -= Time.deltaTime;
        }

        private bool CooldownIsUp() => _currentCooldown <= 0f;
        
        private bool CanAttack() => !_isAttacking && CooldownIsUp();
        
        private bool HasTarget()
        {
            return Physics2D.OverlapCircleNonAlloc(StartPoint(), _radius, _hits, _layerMask) > 0;
        }

        private bool Hit(out Collider2D hit)
        {
            int hitAmount = Physics2D.OverlapCircleNonAlloc(StartPoint(), _radius, _hits, _layerMask);
            hit = _hits.FirstOrDefault();
            return hitAmount > 0;
        }

        private Vector2 StartPoint()
        {
            return new Vector2(transform.position.x, transform.position.y + 0.5f) + 
                   (Vector2)transform.right * _effectiveDistance;
        }

        private void StartAttack()
        {
            _animator.PlayAttack();
            _isAttacking = true;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(StartPoint(), _radius);
        }
    }
}
