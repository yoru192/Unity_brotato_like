using System;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.CritDamage;
using CodeBase.Infrastructure.Services.Settings;
using CodeBase.Logic;
using CodeBase.UI.Elements;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CodeBase.Enemy
{
    public class EnemyHealth : MonoBehaviour, IHealth
    {
        [SerializeField] private float current;
        [SerializeField] private float max;
        [SerializeField] private GameObject damageNumberPrefab;
        [SerializeField] private Transform damageSpawnPoint;

        [Header("Розкид спавну")]
        [SerializeField] private float spawnOffsetX = 0.3f;
        [SerializeField] private float spawnOffsetY = 0.2f;

        private ICritDamageService _critDamageService;
        private ISettingsService _settings;

        public event Action HealthChanged;

        /// <summary>Ворог отримав урон: (величина урону, чи був це крит).</summary>
        public event Action<float, bool> Damaged;

        public float Current { get => current; set => current = value; }
        public float Max { get => max; set => max = value; }

        private void Awake()
        {
            _critDamageService = AllServices.Container.Single<ICritDamageService>();
            _settings = AllServices.Container.Single<ISettingsService>();
        }

        /// <summary>
        /// Урон без явної ознаки криту: беремо результат останнього кидка із сервісу.
        /// Коректно лише коли кидок і удар відбуваються одразу один за одним (меч).
        /// </summary>
        public void TakeDamage(float damage) =>
            TakeDamage(damage, _critDamageService != null && _critDamageService.LastHitWasCritical);

        public void TakeDamage(float damage, bool isCritical)
        {
            if (Current <= 0) return;

            Vector3 spawnPos = GetSpawnPosition();

            Current -= damage;

            SpawnDamageNumber(damage, spawnPos, isCritical);

            Damaged?.Invoke(damage, isCritical);
            HealthChanged?.Invoke();
        }

        /// <summary>
        /// Урон від статус-ефекту (горіння тощо). На відміну від TakeDamage не шле подію Damaged,
        /// тому не викликає віддачу/тряску і не запускає нові ефекти "при ударі".
        /// </summary>
        public void TakeStatusDamage(float damage)
        {
            if (Current <= 0) return;

            Vector3 spawnPos = GetSpawnPosition();

            Current -= damage;

            SpawnDamageNumber(damage, spawnPos, isCritical: false);
            HealthChanged?.Invoke();
        }

        /// <summary>Атака була спрямована на цього ворога, але промазала. HP не змінюється, крит не рахуємо.</summary>
        public void Miss()
        {
            if (Current <= 0) return;

            Vector3 spawnPos = GetSpawnPosition();
            SpawnDamageNumber(0f, spawnPos, isCritical: false, isMiss: true);
        }

        private Vector3 GetSpawnPosition()
        {
            Vector3 basePos = damageSpawnPoint != null
                ? damageSpawnPoint.position
                : transform.position + Vector3.up * 1.5f;

            return basePos + new Vector3(
                Random.Range(-spawnOffsetX, spawnOffsetX),
                Random.Range(0f, spawnOffsetY),
                0f);
        }

        private void SpawnDamageNumber(float damage, Vector3 spawnPos, bool isCritical, bool isMiss = false)
        {
            if (_settings != null && !_settings.DamageNumbers)
                return;
            if (damageNumberPrefab == null)
            {
                Debug.LogWarning("DamageNumber prefab не призначений!", this);
                return;
            }

            if (DamageNumberCanvas.Instance == null)
            {
                Debug.LogWarning("DamageNumberCanvas не знайдений у сцені!", this);
                return;
            }

            DamageNumber number = DamageNumberCanvas.Instance.Spawn(damageNumberPrefab, spawnPos);
            if (number != null)
                number.Initialize(damage, isCritical, isMiss);
        }
    }
}