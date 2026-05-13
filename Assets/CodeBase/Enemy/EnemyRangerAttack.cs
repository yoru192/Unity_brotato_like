using System;
using CodeBase.Common;
using CodeBase.Logic;
using CodeBase.Weapon.RangeWeapon;
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
            _launcher = GetComponent<ProjectileLauncher>();
            _playerLayer = PhysicsLayers.PlayerMask;
            _shootTimer = _shootRate;
        }

        private void Update()
        {
            _shootTimer -= Time.deltaTime;
            if (_shootTimer > 0f) return;
            OnCooldownEnded?.Invoke();
            Transform target = FindTarget();
            if (target == null) return;

            SpawnProjectile(target);
            _shootTimer = _shootRate;
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