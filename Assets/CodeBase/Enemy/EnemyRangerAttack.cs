using CodeBase.Weapon;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class EnemyRangerAttack : MonoBehaviour
    {
        private float _detectionRadius;
        private float _shootRate;
        private float _projectileSpeed;
        [SerializeField] private GameObject _projectilePrefab;

        private float _shootTimer;
        private int _playerLayer;
        private float _damage;

        public void Construct(int damage, float shootRate, float projectileSpeed, float radius)
        {
            _damage = damage;
            _shootRate  = shootRate;
            _projectileSpeed = projectileSpeed;
            _detectionRadius = radius;
            
        }

        private void Awake()
        {
            _playerLayer = 1 << LayerMask.NameToLayer("Player");
            _shootTimer = _shootRate;
        }
        

        private void Update()
        {
            _shootTimer -= Time.deltaTime;
            if (_shootTimer > 0f) return;

            Transform target = FindTarget();
            if (target == null) return;

            _shootTimer = _shootRate;
            SpawnProjectile(target);
        }

        private Transform FindTarget()
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, _detectionRadius, _playerLayer);
            if(hit == null) return null;
            Transform target = null;
            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist > 0)
            {
                target = hit.transform;
            }
            return target;
        }

        private void SpawnProjectile(Transform target)
        {
            var go = Instantiate(_projectilePrefab, transform.position, Quaternion.identity);
            var projectile = go.GetComponent<Projectile>();
            projectile.InitializeProjectile(target, _projectileSpeed, _damage);
        }
    }
}