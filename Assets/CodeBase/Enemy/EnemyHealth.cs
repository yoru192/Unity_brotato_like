using System;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.CritDamage;
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
        private bool _lastHitWasCritical;

        public event Action HealthChanged;

        public float Current { get => current; set => current = value; }
        public float Max { get => max; set => max = value; }

        private void Awake()
        {
            _critDamageService = AllServices.Container.Single<ICritDamageService>();
        }

        private void OnEnable()
        {
            _critDamageService.OnGetCritDamage += RememberCrit;
        }

        private void OnDisable()
        {
            _critDamageService.OnGetCritDamage -= RememberCrit;
        }

        private void RememberCrit(float damage, bool isCritical)
        {
            _lastHitWasCritical = isCritical;
        }

        public void TakeDamage(float damage)
        {
            Vector3 spawnPos = GetSpawnPosition();

            Current -= damage;

            SpawnDamageNumber(damage, spawnPos, _lastHitWasCritical);

            HealthChanged?.Invoke();
        }

        /// <summary>Атака була спрямована на цього ворога, але промазала. HP не змінюється, крит не рахуємо.</summary>
        public void Miss()
        {
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

            GameObject go = Instantiate(
                damageNumberPrefab,
                spawnPos,
                Quaternion.identity,
                DamageNumberCanvas.Instance.Canvas.transform);

            go.GetComponent<DamageNumber>().Initialize(damage, isCritical, isMiss);
        }
    }
}