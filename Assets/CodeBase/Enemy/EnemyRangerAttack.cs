using System;
using CodeBase.Common;
using CodeBase.Logic;
using CodeBase.Weapon.ArrowWeapon;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class EnemyRangerAttack : MonoBehaviour, IAttackWithCooldown
    {
        public event Action OnCooldownStarted;
        public event Action OnCooldownEnded;
        
        private float _detectionRadius;
        private float _shootRate;
        private float _projectileSpeed;
        private float _damage;

        private ProjectileLauncher _launcher;
        private const float TargetSearchInterval = 0.1f;

        private float _shootTimer;
        private bool _isReady;
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
            _launcher = GetComponent<ProjectileLauncher>();
            _playerLayer = PhysicsLayers.PlayerMask;
            _shootTimer = _shootRate;
        }

        private void Update()
        {
            _shootTimer -= Time.deltaTime;
            if (_shootTimer > 0f) return;

            // Подію шлемо один раз на кулдаун, а не щокадру, поки чекаємо на ціль.
            if (!_isReady)
            {
                _isReady = true;
                OnCooldownEnded?.Invoke();
            }

            Transform target = FindTarget();
            if (target == null)
            {
                // Гравець поза радіусом — не скануємо фізику щокадру.
                _shootTimer = TargetSearchInterval;
                return;
            }

            SpawnProjectile(target);
            _shootTimer = _shootRate;
            _isReady = false;
            OnCooldownStarted?.Invoke();
        }

        private Transform FindTarget()
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, _detectionRadius, _playerLayer);
            if (hit == null) return null;
            return hit.transform;
        }

        private void SpawnProjectile(Transform target) =>
            _launcher.Launch(target, _projectileSpeed, _damage);
    }
}