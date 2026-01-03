using System;
using System.Collections.Generic;
using CodeBase.StaticData;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CodeBase.Logic
{
    public class WaveController : MonoBehaviour
    {
        [Serializable]
        public class EnemyWaveConfig
        {
            public EnemyTypeId enemyTypeId;
            public int cost;
        }

        [Header("Wave Settings")]
        public List<EnemyWaveConfig> enemyConfigs = new List<EnemyWaveConfig>();
        public int waveDuration = 60;
        public float spawnInterval = 2f;
        public int maxAlive = 10;

        [Header("Wave Info")]
        public int currentWave;
        private int waveValue;

        private EnemySpawner _spawner;
        private float waveTimer;
        private float spawnTimer;
        private Queue<EnemyTypeId> enemiesToSpawn = new Queue<EnemyTypeId>();

        public event Action OnWaveCompleted;

        private void Awake()
        {
            _spawner = GetComponent<EnemySpawner>();
            if (_spawner == null)
            {
                Debug.LogError("EnemySpawner component not found on the same GameObject!");
            }
        }

        void Start()
        {
            GenerateWave();
        }

        void FixedUpdate()
        {
            if (spawnTimer <= 0)
            {
                if (enemiesToSpawn.Count > 0)
                {
                    TrySpawnNextEnemy();
                    spawnTimer = spawnInterval;
                }
                else
                {
                    waveTimer = 0;
                }
            }
            else
            {
                spawnTimer -= Time.fixedDeltaTime;
                waveTimer -= Time.fixedDeltaTime;
            }

            _spawner.CleanupDeadEnemies();

            if (waveTimer <= 0 && _spawner.GetAliveEnemiesCount() <= 0)
            {
                CompleteWave();
            }
        }

        private async void TrySpawnNextEnemy()
        {
            if (enemiesToSpawn.Count == 0)
                return;

            EnemyTypeId enemyTypeId = enemiesToSpawn.Peek();
            GameObject spawnedEnemy = await _spawner.TrySpawnEnemy(enemyTypeId, maxAlive);

            if (spawnedEnemy != null)
            {
                enemiesToSpawn.Dequeue();
            }
        }

        private void CompleteWave()
        {
            Debug.Log($"Wave {currentWave} completed!");
            OnWaveCompleted?.Invoke();
            currentWave++;
            GenerateWave();
        }

        public void GenerateWave()
        {
            waveValue = currentWave * 10;
            GenerateEnemies();
            waveTimer = waveDuration;
            spawnTimer = 0;

            Debug.Log($"Wave {currentWave} started with {enemiesToSpawn.Count} enemies");
        }

        private void GenerateEnemies()
        {
            List<EnemyTypeId> generatedEnemies = new List<EnemyTypeId>();

            while (waveValue > 0 && generatedEnemies.Count < 50)
            {
                int randEnemyId = Random.Range(0, enemyConfigs.Count);
                int randEnemyCost = enemyConfigs[randEnemyId].cost;

                if (waveValue - randEnemyCost >= 0)
                {
                    generatedEnemies.Add(enemyConfigs[randEnemyId].enemyTypeId);
                    waveValue -= randEnemyCost;
                }
                else
                {
                    break;
                }
            }

            enemiesToSpawn.Clear();
            foreach (var enemy in generatedEnemies)
            {
                enemiesToSpawn.Enqueue(enemy);
            }
        }
    }
}