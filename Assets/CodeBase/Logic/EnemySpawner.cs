using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Enemy;
using CodeBase.Infrastructure.Factory;
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

        public void Construct(IGameFactory gameFactory, List<Vector2> spawnPositions, Transform playerTransform)
        {
            _gameFactory = gameFactory;
            _spawnPositions = spawnPositions;
            player = playerTransform;
        }

        public async Task<GameObject> TrySpawnEnemy(EnemyTypeId enemyTypeId, int maxAlive)
        {
            if (_spawnedEnemies.Count >= maxAlive)
                return null;

            if (_spawnPositions == null || _spawnPositions.Count == 0)
                return null;

            Vector2 spawnPosition = GetValidSpawnPosition();
            GameObject enemy = await _gameFactory.CreateEnemy(enemyTypeId, spawnPosition);

            if (enemy != null)
            {
                _spawnedEnemies.Add(enemy);

                EnemyDeath death = enemy.GetComponent<EnemyDeath>();
                if (death != null)
                {
                    void HandleDying()
                    {
                        _spawnedEnemies.Remove(enemy);
                        death.OnDying -= HandleDying;
                    }
                    death.OnDying += HandleDying;
                }
            }

            return enemy;
        }

        /// <summary>
        /// Спавнить ворога (боса) на точці спавну, чия відстань до гравця найближча до preferredDistance,
        /// без ліміту maxAlive і без обліку в цьому спавнері — щоб звичайні хвилі не чекали на його смерть.
        /// </summary>
        public Task<GameObject> SpawnUntracked(EnemyTypeId enemyTypeId, float preferredDistance)
        {
            return _gameFactory.CreateEnemy(enemyTypeId, SpawnPositionNearDistance(preferredDistance));
        }

        private Vector2 SpawnPositionNearDistance(float preferredDistance)
        {
            if (_spawnPositions == null || _spawnPositions.Count == 0)
                return player != null ? (Vector2)player.position + Vector2.right * preferredDistance : Vector2.zero;
            if (player == null)
                return _spawnPositions[0];

            Vector2 best = _spawnPositions[0];
            float bestError = float.MaxValue;
            foreach (Vector2 position in _spawnPositions)
            {
                float error = Mathf.Abs(Vector2.Distance(position, player.position) - preferredDistance);
                if (error < bestError)
                {
                    bestError = error;
                    best = position;
                }
            }
            return best;
        }

        /// <summary>Добиває всіх живих ворогів зі спавнера (після смерті боса). Нагороди падають як завжди.</summary>
        public void KillAll()
        {
            // Копія: смерть прибирає ворога з _spawnedEnemies через OnDying.
            GameObject[] alive = _spawnedEnemies.ToArray();
            foreach (GameObject enemy in alive)
            {
                if (enemy == null || !enemy.activeInHierarchy) continue;
                if (enemy.TryGetComponent(out EnemyHealth health) && health.Current > 0f)
                    health.TakeStatusDamage(health.Current);
            }
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
            _spawnedEnemies.RemoveAll(enemy => enemy == null || !enemy.activeInHierarchy);
        }
        
        public void ReturnAllEnemies()
        {
            foreach (var enemy in _spawnedEnemies)
            {
                if (enemy != null && enemy.activeInHierarchy)
                    ObjectPoolManager.ReturnObjectToPool(enemy, ObjectPoolManager.PoolType.Enemy);
            }
            _spawnedEnemies.Clear();
        }

        public int GetAliveEnemiesCount()
        {
            return _spawnedEnemies.Count;
        }
    }
}