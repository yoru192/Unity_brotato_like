using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Weapon
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float _destroyDistance = 0.15f;

        private Transform _target;
        private float _moveSpeed;
        private float _damage;

        public void InitializeProjectile(Transform target, float moveSpeed, float damage)
        {
            _target    = target;
            _moveSpeed = moveSpeed;
            _damage    = damage;
        }

        private void Update()
        {
            if (_target == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 dir = (_target.position - transform.position).normalized;
            transform.position += dir * (_moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, _target.position) < _destroyDistance)
            {
                _target.GetComponentInParent<IHealth>()?.TakeDamage(_damage);
                Destroy(gameObject);
            }
        }
    }
}