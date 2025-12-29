using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Logic
{
    public class WaveSpawner : MonoBehaviour
    {
        [System.Serializable]
        public class EnemyWaveConfig
        {
            public EnemyTypeId enemyTypeId;
            public int cost;
        }

        public List<EnemyWaveConfig> enemyConfigs = new List<EnemyWaveConfig>();
        public int currWave;
        private int waveValue;
        public Transform[] spawnLocation;
        public int spawnIndex;
        public int waveDuration;
        
        private IGameFactory _gameFactory;
        private float waveTimer;
        private float spawnInterval;
        private float spawnTimer;
        private Queue<EnemyTypeId> enemiesToSpawn = new Queue<EnemyTypeId>();
        public List<GameObject> spawnedEnemies = new List<GameObject>();

        public void Construct(IGameFactory gameFactory)
        {
            _gameFactory = gameFactory;
        }

        void Start()
        {
            _gameFactory = AllServices.Container.Single<IGameFactory>();
            GenerateWave();
        }

        void FixedUpdate()
        {
            if (spawnTimer <= 0)
            {
                if (enemiesToSpawn.Count > 0)
                {
                    SpawnNextEnemy();
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

            spawnedEnemies.RemoveAll(enemy => enemy == null);

            if (waveTimer <= 0 && spawnedEnemies.Count <= 0)
            {
                currWave++;
                GenerateWave();
            }
        }

        private async void SpawnNextEnemy()
        {
            if (spawnLocation == null || spawnLocation.Length == 0)
            {
                Debug.LogError("Spawn locations are not set!");
                return;
            }

            if (spawnIndex < 0 || spawnIndex >= spawnLocation.Length)
            {
                spawnIndex = 0;
            }

            if (spawnLocation[spawnIndex] == null)
            {
                Debug.LogError($"Spawn location at index {spawnIndex} is null!");
                spawnIndex = (spawnIndex + 1) % spawnLocation.Length;
                return;
            }

            EnemyTypeId enemyTypeId = enemiesToSpawn.Dequeue();

            GameObject enemy = await _gameFactory.CreateEnemy(enemyTypeId, spawnLocation[spawnIndex]);
            
            if (enemy != null)
            {
                spawnedEnemies.Add(enemy);
            }

            spawnIndex = (spawnIndex + 1) % spawnLocation.Length;
        }

        public void GenerateWave()
        {
            waveValue = currWave * 10;
            GenerateEnemies();
            spawnInterval = enemiesToSpawn.Count > 0 ? (float)waveDuration / enemiesToSpawn.Count : 1f;
            waveTimer = waveDuration;
        }

        public void GenerateEnemies()
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
