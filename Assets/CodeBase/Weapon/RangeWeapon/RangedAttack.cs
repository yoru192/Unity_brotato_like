using CodeBase.Common;
using CodeBase.Data;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Audio;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Weapon.RangeWeapon
{
    public class RangedAttack : WeaponBase, ISavedProgress
    {
        private IAudioService _audioService;
        private State _state;
        private float _detectionRadius;
        private float _shootRate;
        private float _projectileMaxMoveSpeed;
        private float _damage;

        private ProjectileLauncher _launcher;
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
            _audioService = AllServices.Container.Single<IAudioService>();
            _launcher = GetComponent<ProjectileLauncher>();
            _hittableLayer = PhysicsLayers.HittableMask;
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
            _audioService?.PlaySfx(AudioClipId.ArrowShoot);
        }

        private void SpawnProjectile(Transform target) =>
            _launcher.Launch(target, _projectileMaxMoveSpeed, Damage);
            

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