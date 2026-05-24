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
        private int _maxWaves;

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

        public int MaxWaves => _maxWaves;

        public event Action OnWaveCompleted;
        public event Action OnRunCompleted;

        public void Construct(WaveControllerStaticData waveControllerData)
        {
            _waveControllerData  = waveControllerData;
            _waveValueMultiplier = _waveControllerData.waveValueMultiplier;
            _maxEnemiesPerWave   = _waveControllerData.maxEnemiesPerWave;
            _initialSpawnTimer   = _waveControllerData.initialSpawnTimer;
            _maxWaves            = _waveControllerData.maxWaves;
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

            bool waveCleared = waveTimer <= 0 && enemiesToSpawn.Count == 0 && !_isSpawning && _spawner.GetAliveEnemiesCount() <= 0;
            if ((!_isWaveCompleting && waveCleared) || (currentWave == 0 && _initialSpawnTimer <= 0))
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
                // якщо відхилено через maxAlive — не чекаємо spawnInterval, пробуємо швидко
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
            currentWave++;

            if (currentWave >= _maxWaves)
            {
                OnRunCompleted?.Invoke();
                _isWaveCompleting = false;
                return;
            }

            OnWaveCompleted?.Invoke();
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

            while (_waveValue > 0 && generatedEnemies.Count < _maxEnemiesPerWave)
            {
                List<EnemyWaveConfig> affordable = enemyConfigs.Where(e => e.cost <= _waveValue).ToList();
                if (affordable.Count == 0) break;

                EnemyWaveConfig chosen = affordable[Random.Range(0, affordable.Count)];
                generatedEnemies.Add(chosen.enemyTypeId);
                _waveValue -= chosen.cost;
            }

            enemiesToSpawn.Clear();
            foreach (var enemy in generatedEnemies)
                enemiesToSpawn.Enqueue(enemy);
        }
    }
}
