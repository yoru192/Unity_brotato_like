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

        [Header("Off-screen spawning (arena scenes)")]
        [Tooltip("How far outside the camera view enemies appear, in units.")]
        [SerializeField] private float offscreenMargin = 3f;
        [Tooltip("Keep spawns this far inside the arena bounds.")]
        [SerializeField] private float boundsInset = 1f;

        private IGameFactory _gameFactory;
        private List<GameObject> _spawnedEnemies = new List<GameObject>();
        private List<Vector2> _spawnPositions;

        private bool _spawnOffscreen;
        private Rect _arenaBounds;
        private Camera _camera;

        public void Construct(IGameFactory gameFactory, List<Vector2> spawnPositions, Transform playerTransform)
        {
            _gameFactory = gameFactory;
            _spawnPositions = spawnPositions;
            player = playerTransform;
        }

        /// <summary>
        /// Spawns enemies on a ring just outside the camera view around the player (clamped to the arena)
        /// instead of the fixed spawn points.
        /// </summary>
        public void SpawnOffscreen(Rect arenaBounds, Camera viewCamera)
        {
            _spawnOffscreen = true;
            _arenaBounds = arenaBounds;
            _camera = viewCamera;
        }

        public async Task<GameObject> TrySpawnEnemy(EnemyTypeId enemyTypeId, int maxAlive)
        {
            if (_spawnedEnemies.Count >= maxAlive)
                return null;

            if (!_spawnOffscreen && (_spawnPositions == null || _spawnPositions.Count == 0))
                return null;

            Vector2 spawnPosition = _spawnOffscreen ? OffscreenSpawnPosition() : GetValidSpawnPosition();
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
            if (_spawnOffscreen && player != null)
                return ClampToArena((Vector2)player.position + Random.insideUnitCircle.normalized * preferredDistance);

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

        private Vector2 OffscreenSpawnPosition()
        {
            // Centre on the camera, not the player: near the arena edge the camera is clamped and the
            // player is no longer in the middle of the view.
            Vector2 center = _camera != null ? (Vector2)_camera.transform.position
                : player != null ? (Vector2)player.position : _arenaBounds.center;
            float halfHeight = _camera != null ? _camera.orthographicSize : 20f;
            float halfWidth = _camera != null ? halfHeight * _camera.aspect : halfHeight * 16f / 9f;
            Vector2 half = new Vector2(halfWidth + offscreenMargin, halfHeight + offscreenMargin);

            // A random point on the rectangle just outside the view. Near the arena edge one side may be
            // outside the walls; clamping would pull it back on screen, so retry a few times.
            Vector2 candidate = center;
            for (int attempt = 0; attempt < 8; attempt++)
            {
                candidate = ClampToArena(center + PointOnRectPerimeter(half));
                Vector2 offset = candidate - center;
                if (Mathf.Abs(offset.x) >= half.x - 0.01f || Mathf.Abs(offset.y) >= half.y - 0.01f)
                    return candidate;
            }

            return candidate;
        }

        private static Vector2 PointOnRectPerimeter(Vector2 half)
        {
            float perimeter = 4f * (half.x + half.y);
            float t = Random.value * perimeter;

            if (t < 2f * half.x) return new Vector2(-half.x + t, half.y);
            t -= 2f * half.x;
            if (t < 2f * half.x) return new Vector2(-half.x + t, -half.y);
            t -= 2f * half.x;
            if (t < 2f * half.y) return new Vector2(-half.x, -half.y + t);
            t -= 2f * half.y;
            return new Vector2(half.x, -half.y + t);
        }

        private Vector2 ClampToArena(Vector2 position)
        {
            if (!_spawnOffscreen)
                return position;

            return new Vector2(
                Mathf.Clamp(position.x, _arenaBounds.xMin + boundsInset, _arenaBounds.xMax - boundsInset),
                Mathf.Clamp(position.y, _arenaBounds.yMin + boundsInset, _arenaBounds.yMax - boundsInset));
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