using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase.StaticData;
using CodeBase.StaticData.Enemy;
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

        private int _waveValueMultiplier;
        private int _maxEnemiesPerWave;
        private float _initialSpawnTimer;

        [Header("Wave Settings")]
        public List<EnemyWaveConfig> enemyConfigs = new List<EnemyWaveConfig>();
        public int waveDuration;
        public float spawnInterval;
        public int maxAlive;

        [Header("Wave Info")]
        public int currentWave;
        public int waveBudget;
        
        private int _waveValue;
        private EnemySpawner _spawner;
        private float waveTimer;
        private float spawnTimer;
        private Queue<EnemyTypeId> enemiesToSpawn = new Queue<EnemyTypeId>();
        private WaveControllerStaticData _waveControllerData;
        private bool _isSpawning = false;
        private bool _isWaveCompleting = false;

        public event Action OnWaveCompleted;

        public void Construct(WaveControllerStaticData waveControllerData)
        {
            _waveControllerData  = waveControllerData;
            _waveValueMultiplier = _waveControllerData.waveValueMultiplier;
            _maxEnemiesPerWave   = _waveControllerData.maxEnemiesPerWave;
            _initialSpawnTimer   = _waveControllerData.initialSpawnTimer;
            waveDuration         = _waveControllerData.waveDuration;
            spawnInterval        = _waveControllerData.spawnInterval;
            maxAlive             = _waveControllerData.maxAlive;
            enemyConfigs         = _waveControllerData.enemyConfigs
                .Select(e => new EnemyWaveConfig { enemyTypeId = e.enemyTypeId, cost = e.cost })
                .ToList();

            GenerateWave();
        }


        private void Awake()
        {
            _spawner = GetComponent<EnemySpawner>();
            if (_spawner == null)
            {
                Debug.LogError("EnemySpawner component not found on the same GameObject!");
            }
        }
        
        void FixedUpdate()
        {
            _initialSpawnTimer -= Time.fixedDeltaTime;
            waveTimer -= Time.fixedDeltaTime;


            if (spawnTimer > 0)
                spawnTimer -= Time.fixedDeltaTime;

            if (!_isSpawning && spawnTimer <= 0 && enemiesToSpawn.Count > 0)
                TrySpawnNextEnemy();

            _spawner.CleanupDeadEnemies();

            if ((waveTimer <= 0 && !_isWaveCompleting && _spawner.GetAliveEnemiesCount() <= 0) || currentWave == 0 && _initialSpawnTimer <= 0)
                CompleteWave();
        }


        private async void TrySpawnNextEnemy()
        {
            if (enemiesToSpawn.Count == 0 || _isSpawning) return;

            _isSpawning = true;
            try
            {
                EnemyTypeId enemyTypeId = enemiesToSpawn.Peek();
                GameObject spawnedEnemy = await _spawner.TrySpawnEnemy(enemyTypeId, maxAlive);

                if (spawnedEnemy != null)
                {
                    enemiesToSpawn.Dequeue();
                    spawnTimer = spawnInterval;
                }
                else
                {
                    spawnTimer = spawnInterval;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to spawn enemy: {e.Message}");
                spawnTimer = spawnInterval;
            }
            finally
            {
                _isSpawning = false;
            }
        }


        private void CompleteWave()
        {
            _isWaveCompleting = true;
            OnWaveCompleted?.Invoke();
            currentWave++;
            GenerateWave();
            _isWaveCompleting = false;
        }

        public void GenerateWave()
        {
            _waveValue = currentWave * _waveValueMultiplier;
            waveBudget = _waveValue;
            GenerateEnemies();
            waveTimer = waveDuration;
            spawnTimer = spawnInterval;
            Debug.Log($"Wave {currentWave} started with {enemiesToSpawn.Count} enemies");
        }

        private void GenerateEnemies()
        {
            List<EnemyTypeId> generatedEnemies = new List<EnemyTypeId>();
            int minCost = enemyConfigs.Min(e => e.cost);
            
            while (_waveValue > 0 && generatedEnemies.Count < _maxEnemiesPerWave)
            {
                int randEnemyId = Random.Range(0, enemyConfigs.Count);
                int randEnemyCost = enemyConfigs[randEnemyId].cost;

                if (_waveValue - randEnemyCost >= 0)
                {
                    generatedEnemies.Add(enemyConfigs[randEnemyId].enemyTypeId);
                    _waveValue -= randEnemyCost;
                }
                else
                {
                    if (_waveValue < minCost) break;
                }
            }

            enemiesToSpawn.Clear();
            foreach (var enemy in generatedEnemies)
            {
                enemiesToSpawn.Enqueue(enemy);
            }
        }
        
        public Dictionary<EnemyTypeId, float> GetSpawnStatistics()
        {
            var stats = new Dictionary<EnemyTypeId, float>();
            if (enemyConfigs == null || enemyConfigs.Count == 0) return stats;

            int totalBudget = currentWave * _waveValueMultiplier;
            if (totalBudget == 0) return stats;

            foreach (var config in enemyConfigs)
            {
                float countPerBudget = (float)totalBudget / config.cost;
                stats[config.enemyTypeId] = countPerBudget;
            }

            float total = stats.Values.Sum();
            foreach (var key in stats.Keys.ToList())
                stats[key] = Mathf.Round(stats[key] / total * 100f);

            return stats;
        }

    }
}
