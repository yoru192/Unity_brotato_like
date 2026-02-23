using System;
using CodeBase.Logic;
using CodeBase.UI.Elements;
using UnityEngine;

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

        public event Action HealthChanged;

        public float Current
        {
            get => current;
            set => current = value;
        }

        public float Max
        {
            get => max;
            set => max = value;
        }

        public void TakeDamage(float damage)
        {
            // Зберігаємо позицію ДО того як ворог може бути знищений
            Vector3 spawnPos = GetSpawnPosition();

            Current -= damage;

            SpawnDamageNumber(damage, spawnPos);

            HealthChanged?.Invoke();
        }

        private Vector3 GetSpawnPosition()
        {
            Vector3 basePos = damageSpawnPoint != null
                ? damageSpawnPoint.position
                : transform.position + Vector3.up * 1.5f;

            return basePos + new Vector3(
                UnityEngine.Random.Range(-spawnOffsetX, spawnOffsetX),
                UnityEngine.Random.Range(0f, spawnOffsetY),
                0f
            );
        }

        private void SpawnDamageNumber(float damage, Vector3 spawnPos)
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

            // Спавнимо у глобальний Canvas — він не знищується разом з ворогом
            GameObject go = Instantiate(
                damageNumberPrefab,
                spawnPos,
                Quaternion.identity,
                DamageNumberCanvas.Instance.Canvas.transform
            );

            bool isCritical = damage >= 50;
            go.GetComponent<DamageNumber>().Initialize(damage, isCritical);
        }
    }
}