using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Infrastructure.Factory;
using CodeBase.StaticData;
using CodeBase.StaticData.Enemy;
using UnityEngine;

namespace CodeBase.Logic
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        public Transform player;
        public float minDistanceFromPlayer = 5f;
        
        private IGameFactory _gameFactory;
        private List<GameObject> _spawnedEnemies = new List<GameObject>();
        private List<Vector2> _spawnPositions;

        public void Construct(IGameFactory gameFactory, List<Vector2> spawnPositions)
        {
            _gameFactory = gameFactory;
            _spawnPositions = spawnPositions;
        }

        private void Start()
        {
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                }
                else
                {
                    Debug.LogWarning("Player not assigned and no GameObject with 'Player' tag found!");
                }
            }
        }

        public async Task<GameObject> TrySpawnEnemy(EnemyTypeId enemyTypeId, int maxAlive)
        {
            if (_spawnedEnemies.Count >= maxAlive)
            {
                return null;
            }

            if (_spawnPositions == null || _spawnPositions.Count == 0)
            {
                return null;
            }

            Vector2 spawnPosition = GetValidSpawnPosition();
            
            GameObject tempSpawnPoint = new GameObject("TempSpawnPoint");
            tempSpawnPoint.transform.position = spawnPosition;
            
            GameObject enemy = await _gameFactory.CreateEnemy(enemyTypeId, tempSpawnPoint.transform);
            
            Destroy(tempSpawnPoint);
            
            if (enemy != null)
            {
                _spawnedEnemies.Add(enemy);
            }

            return enemy;
        }

        private Vector2 GetValidSpawnPosition()
        {
            if (_spawnPositions.Count == 0)
                return Vector2.zero;

            int maxAttempts = _spawnPositions.Count * 2;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                Vector2 spawnPos = _spawnPositions[Random.Range(0, _spawnPositions.Count)];

                if (player != null)
                {
                    float distance = Vector2.Distance(spawnPos, player.position);
                    if (distance >= minDistanceFromPlayer)
                    {
                        return spawnPos;
                    }
                }
                else
                {
                    return spawnPos;
                }

                attempts++;
            }

            Debug.LogWarning($"Could not find spawn point with minimum distance {minDistanceFromPlayer} from player after {maxAttempts} attempts");
            return _spawnPositions[Random.Range(0, _spawnPositions.Count)];
        }

        public void CleanupDeadEnemies()
        {
            _spawnedEnemies.RemoveAll(enemy => enemy == null);
        }
        
        public void DestroyAllEnemies()
        {
            foreach (var enemy in _spawnedEnemies)
            {
                if (enemy != null)
                    Destroy(enemy);
            }
            _spawnedEnemies.Clear();
        }

        public int GetAliveEnemiesCount()
        {
            return _spawnedEnemies.Count;
        }
        
    }
}
