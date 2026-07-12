using CodeBase.Common;
using CodeBase.Data;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Audio;
using CodeBase.Infrastructure.Services.CritDamage;
using CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;

namespace CodeBase.Weapon.ArrowWeapon
{
    public class ArrowAttack : WeaponBase, ISavedProgress
    {
        [Header("Miss Chance")]
        [Tooltip("Дистанція, на якій і ближче — 100% влучення (промах неможливий).")]
        [SerializeField] private float _guaranteedHitRadius = 3f;

        [Tooltip("Максимальний шанс промаху (%) на максимальній дистанції виявлення цілі (_detectionRadius).")]
        [Range(0f, 100f)]
        [SerializeField] private float _maxMissChance = 70f;

        private IAudioService _audioService;
        private State _state;
        private float _detectionRadius;
        private float _shootRate;
        private float _projectileMaxMoveSpeed;
        private float _damage;

        private ProjectileLauncher _launcher;
        private float _shootTimer;
        private int _hittableLayer;
        private int _critChance;
        private float _critRate;
        private ICritDamageService _critDamageService;

        public int Damage
        {
            get => (int)(_state?.ArrowWeaponState.weaponDamage ?? _damage);
            set
            {
                if (_state != null && _state.ArrowWeaponState.weaponDamage != value)
                    _state.ArrowWeaponState.weaponDamage = value;
            }
        }

        public float Cooldown
        {
            get => _state?.ArrowWeaponState.weaponCooldown ?? _shootRate;
            set
            {
                if (_state != null && _state.ArrowWeaponState.weaponCooldown != value)
                    _state.ArrowWeaponState.weaponCooldown = value;
            }
        }

        public void Construct(int damage, float shootRate, float projectileSpeed, float radius, int critChance,
            float critRate, ICritDamageService critDamageService)
        {
            _damage = damage;
            _shootRate = shootRate;
            _projectileMaxMoveSpeed = projectileSpeed;
            _detectionRadius = radius;
            _critChance = critChance;
            _critRate = critRate;
            _critDamageService = critDamageService;
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

            float distance = Vector2.Distance(transform.position, nearestTarget.position);

            // t = 0 на _guaranteedHitRadius (і ближче), t = 1 на _detectionRadius.
            // Використовуємо його і для шансу промаху, і для того, наскільки сильно
            // відхиляється точка прильоту, якщо промах усе-таки станеться.
            float t = CalculateMissT(distance);
            bool isMiss = RollMiss(t);

            SpawnProjectile(nearestTarget, isMiss, t);
            _audioService?.PlaySfx(AudioClipId.ArrowShoot);
        }

        private float CalculateMissT(float distance)
        {
            if (distance <= _guaranteedHitRadius)
                return 0f;

            return Mathf.InverseLerp(_guaranteedHitRadius, _detectionRadius, distance);
        }

        private bool RollMiss(float t)
        {
            float missChance = t * _maxMissChance;
            if (missChance <= 0f)
                return false;

            int randomValue = Random.Range(0, 100);
            // randomValue < missChance -> потрапляємо у "промаховий" діапазон.
            return randomValue < missChance;
        }

        private float ApplyCritDamage() =>
            _critDamageService.GetCritDamage(Damage, _critChance, _critRate);

        private void SpawnProjectile(Transform target, bool isMiss, float missSeverity)
        {
            // При промаху урон не рахуємо взагалі — Projectile сам покаже "Miss!" через IHealth.Miss()
            // і відхилить точку прильоту (тим сильніше, чим більша missSeverity), замість накладання
            // нульового урону.
            float damage = isMiss ? 0f : ApplyCritDamage();
            _launcher.Launch(target, _projectileMaxMoveSpeed, damage, isMiss, missSeverity);
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

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _guaranteedHitRadius);
        }

        public void LoadProgress(PlayerProgress progress)
        {
            _state = progress.playerState;
            if (_state.ArrowWeaponState.weaponDamage == 0 && _damage > 0)
                _state.ArrowWeaponState.weaponDamage = (int)_damage;
            if (_state.ArrowWeaponState.weaponCooldown == 0 && _shootRate > 0)
                _state.ArrowWeaponState.weaponCooldown = _shootRate;
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            progress.playerState.ArrowWeaponState.weaponDamage = Damage;
            progress.playerState.ArrowWeaponState.weaponCooldown = Cooldown;
        }
    }
}