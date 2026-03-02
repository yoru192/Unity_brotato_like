using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;

namespace CodeBase.Weapon.RangeWeapon
{
    public class RangedAttack : WeaponBase, ISavedProgress
    {
        private State _state;
        private float _detectionRadius;
        private float _shootRate;
        private float _projectileMaxMoveSpeed;
        private float _damage;

        [SerializeField] private GameObject _projectilePrefab;

        [Header("Arc")]
        [Tooltip("Висота дуги снаряда у world units.")]
        [SerializeField] private float _arcHeight = 2f;

        [Header("Speed Curve (опційно)")]
        [Tooltip("Крива швидкості снаряда. Залиш пустою для постійної швидкості.")]
        [SerializeField] private AnimationCurve _speedCurve;

        private float _shootTimer;
        private int _hittableLayer;

        public int Damage
        {
            get => (int)(_state?.RangedWeaponState.weaponDamage ?? _damage);
            set
            {
                if (_state != null && _state.RangedWeaponState.weaponDamage != value)
                    _state.RangedWeaponState.weaponDamage = value;
            }
        }

        public float Cooldown
        {
            get => _state?.RangedWeaponState.weaponCooldown ?? _shootRate;
            set
            {
                if (_state != null && _state.RangedWeaponState.weaponCooldown != value)
                    _state.RangedWeaponState.weaponCooldown = value;
            }
        }

        public void Construct(int damage, float shootRate, float projectileSpeed, float radius)
        {
            _damage = damage;
            _shootRate = shootRate;
            _projectileMaxMoveSpeed = projectileSpeed;
            _detectionRadius = radius;
        }

        private void Awake()
        {
            _hittableLayer = 1 << LayerMask.NameToLayer("Hittable");
            _shootTimer = _shootRate;
        }

        private void Update()
        {
            _shootTimer -= Time.deltaTime;
            if (_shootTimer > 0f) return;

            Transform nearestTarget = FindNearestTarget();
            if (nearestTarget == null) return;

            _shootTimer = Cooldown;
            SpawnProjectile(nearestTarget);
        }

        private void SpawnProjectile(Transform target)
        {
            var go = Instantiate(_projectilePrefab, transform.position, Quaternion.identity);
            var projectile = go.GetComponent<Projectile>();

            projectile.InitializeProjectile(target, _projectileMaxMoveSpeed, _arcHeight, Damage);

            if (_speedCurve != null && _speedCurve.keys.Length > 0)
                projectile.InitializeSpeedCurve(_speedCurve);
        }

        private Transform FindNearestTarget()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _detectionRadius, _hittableLayer);
            if (hits.Length == 0) return null;

            Transform nearest = null;
            float minDist = float.MaxValue;

            foreach (var hit in hits)
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = hit.transform;
                }
            }

            return nearest;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectionRadius);
        }

        public void LoadProgress(PlayerProgress progress)
        {
            _state = progress.playerState;
            if (_state.RangedWeaponState.weaponDamage == 0 && _damage > 0)
                _state.RangedWeaponState.weaponDamage = (int)_damage;
            if (_state.RangedWeaponState.weaponCooldown == 0 && _shootRate > 0)
                _state.RangedWeaponState.weaponCooldown = _shootRate;
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            progress.playerState.RangedWeaponState.weaponDamage = Damage;
            progress.playerState.RangedWeaponState.weaponCooldown = Cooldown;
        }
    }
}