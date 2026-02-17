using CodeBase.Logic;
using CodeBase.Player;
using UnityEngine;

namespace CodeBase.Enemy
{
    [RequireComponent(typeof(EnemyHealth))]
    [RequireComponent(typeof(EnemyDeath))]
    public class EnemyKamikadzeAttack : MonoBehaviour
    {
        private Transform _playerTransform;
        private int _explosionDamage;
        private bool _hasExploded;
        private EnemyDeath _enemyDeath;
        public GameObject explosionPrefab;

        public void Construct(Transform playerTransform, int damage)
        {
            _playerTransform = playerTransform;
            _explosionDamage = damage;
        }

        private void Awake()
        {
            _enemyDeath = GetComponent<EnemyDeath>();
        }

        private void Start()
        {
            _enemyDeath.OnDying += OnEnemyDying;
        }

        private void OnDestroy()
        {
            if (_enemyDeath != null)
                _enemyDeath.OnDying -= OnEnemyDying;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Explode(collision.gameObject);
            }
        }

        private void OnEnemyDying()
        {
            if (_hasExploded) return;
            
            if (_playerTransform != null)
            {
                if (_playerTransform.TryGetComponent<PlayerHealth>(out var playerHealth))
                {
                    playerHealth.TakeDamage(_explosionDamage);
                }
            }
            
            _hasExploded = true;
            SpawnExplosion();
        }

        private void Explode(GameObject player)
        {
            if (_hasExploded) return;
            _hasExploded = true;
            
            if (player.TryGetComponent<PlayerHealth>(out var health))
            {
                health.TakeDamage(_explosionDamage);
            }
            
            SpawnExplosion();
            Destroy(gameObject);
        }

        private void SpawnExplosion()
        {
            if (explosionPrefab != null)
            {
                GameObject explosionEffect = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(explosionEffect, 1f);
            }
        }
        
    }
}
