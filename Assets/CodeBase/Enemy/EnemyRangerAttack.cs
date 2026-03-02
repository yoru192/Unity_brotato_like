using CodeBase.Weapon.RangeWeapon;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class EnemyRangerAttack : MonoBehaviour
    {
        private float _detectionRadius;
        private float _shootRate;
        private float _projectileSpeed;
        private float _damage;

        [SerializeField] private GameObject _projectilePrefab;

        [Header("Arc")]
        [Tooltip("Висота дуги снаряда у world units.")]
        [SerializeField] private float _arcHeight = 2f;

        [Header("Speed Curve (опційно)")]
        [Tooltip("Крива швидкості снаряда. X=прогрес польоту, Y=множник швидкості. " +
                 "Залиш пустою для постійної швидкості.")]
        [SerializeField] private AnimationCurve _speedCurve;

        private float _shootTimer;
        private int _playerLayer;

        public void Construct(int damage, float shootRate, float projectileSpeed, float radius)
        {
            _damage = damage;
            _shootRate = shootRate;
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
            if (hit == null) return null;
            return hit.transform;
        }

        private void SpawnProjectile(Transform target)
        {
            var go = Instantiate(_projectilePrefab, transform.position, Quaternion.identity);
            var projectile = go.GetComponent<Projectile>();

            projectile.InitializeProjectile(target, _projectileSpeed, _arcHeight, _damage);
            
            if (_speedCurve != null && _speedCurve.keys.Length > 0)
                projectile.InitializeSpeedCurve(_speedCurve);
        }
    }
}