using System.Collections.Generic;
using CodeBase.Common;
using CodeBase.Data;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Audio;
using CodeBase.Infrastructure.Services.CritDamage;
using CodeBase.Infrastructure.Services.Perk;
using CodeBase.StaticData.Perk;
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

        private const float TargetSearchInterval = 0.1f;

        // Спільний буфер для пошуку цілей: без алокації масиву на кожен запит.
        private static readonly List<Collider2D> TargetBuffer = new List<Collider2D>(64);

        private const int MaxExtraArrows = 8;

        // Точка відліку для сортування цілей; статична, щоб компаратор не захоплював змінні (без алокацій).
        private static Vector2 _sortOrigin;
        private static readonly System.Comparison<Collider2D> ByDistanceToSortOrigin = CompareByDistanceToSortOrigin;

        private ContactFilter2D _targetFilter;
        private IPerkService _perkService;
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
            _perkService = AllServices.Container.Single<IPerkService>();
            _launcher = GetComponent<ProjectileLauncher>();
            _hittableLayer = PhysicsLayers.HittableMask;
            _targetFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = _hittableLayer,
                useTriggers = Physics2D.queriesHitTriggers
            };
            _shootTimer = _shootRate;
        }

        private void Update()
        {
            _shootTimer -= Time.deltaTime;
            if (_shootTimer > 0f) return;

            // Перк MultiShot: додаткові стріли летять у наступних за близькістю ворогів.
            int arrows = 1 + ExtraArrows();

            int targetCount = CollectTargets(sortByDistance: arrows > 1);
            if (targetCount == 0)
            {
                // Цілей немає — не скануємо фізику щокадру, а пробуємо знову через короткий інтервал.
                _shootTimer = TargetSearchInterval;
                return;
            }

            _shootTimer = Cooldown;

            // Якщо ворогів менше, ніж стріл, зайві стріли йдуть по колу в тих самих ворогів.
            for (int i = 0; i < arrows; i++)
                ShootAt(TargetBuffer[i % targetCount].transform);

            TargetBuffer.Clear();
            _audioService?.PlaySfx(AudioClipId.ArrowShoot);
        }

        private int ExtraArrows()
        {
            if (_perkService == null) return 0;
            return Mathf.Clamp(Mathf.RoundToInt(_perkService.GetValue(PerkId.MultiShot)), 0, MaxExtraArrows);
        }

        private void ShootAt(Transform target)
        {
            float distance = Vector2.Distance(transform.position, target.position);

            // t = 0 на _guaranteedHitRadius (і ближче), t = 1 на _detectionRadius.
            // Використовуємо його і для шансу промаху, і для того, наскільки сильно
            // відхиляється точка прильоту, якщо промах усе-таки станеться.
            float t = CalculateMissT(distance);
            bool isMiss = RollMiss(t);

            SpawnProjectile(target, isMiss, t);
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

            // Крит кидається тут, а влучання станеться пізніше — тому ознаку криту везе сама стріла.
            bool isCritical = !isMiss && _critDamageService.LastHitWasCritical;

            _launcher.Launch(target, _projectileMaxMoveSpeed, damage, isMiss, missSeverity, isCritical);
        }

        /// <summary>
        /// Заповнює TargetBuffer ворогами в радіусі та повертає їх кількість. Найближчий завжди на позиції 0;
        /// із sortByDistance увесь буфер відсортовано від найближчого. Викликач має очистити буфер після себе.
        /// </summary>
        private int CollectTargets(bool sortByDistance)
        {
            Vector2 origin = transform.position;
            int count = Physics2D.OverlapCircle(origin, _detectionRadius, _targetFilter, TargetBuffer);
            if (count <= 1) return count;

            if (sortByDistance)
            {
                _sortOrigin = origin;
                TargetBuffer.Sort(ByDistanceToSortOrigin);
                return count;
            }

            int nearestIndex = 0;
            float minSqrDist = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                float sqrDist = ((Vector2)TargetBuffer[i].transform.position - origin).sqrMagnitude;
                if (sqrDist < minSqrDist)
                {
                    minSqrDist = sqrDist;
                    nearestIndex = i;
                }
            }

            if (nearestIndex != 0)
                (TargetBuffer[0], TargetBuffer[nearestIndex]) = (TargetBuffer[nearestIndex], TargetBuffer[0]);

            return count;
        }

        private static int CompareByDistanceToSortOrigin(Collider2D a, Collider2D b)
        {
            float distA = ((Vector2)a.transform.position - _sortOrigin).sqrMagnitude;
            float distB = ((Vector2)b.transform.position - _sortOrigin).sqrMagnitude;
            return distA.CompareTo(distB);
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