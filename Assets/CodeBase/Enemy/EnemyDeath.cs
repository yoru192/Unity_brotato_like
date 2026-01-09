using System;
using System.Collections;
using CodeBase.Infrastructure.Services;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Enemy
{
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyDeath : MonoBehaviour
    {
        public EnemyHealth health;
        private IProgressService _progressService;
        private EnemyStaticData _enemyData;
        public event Action OnDying;

        public void Construct(IProgressService progressService, EnemyStaticData enemyData)
        {
            _progressService = progressService;
            _enemyData = enemyData;
        }
        private void Start()
        {
            health.HealthChanged += HealthChanged;
        }

        private void OnDestroy()
        {
            health.HealthChanged -= HealthChanged;
        }
        private void HealthChanged()
        {
            if (health.Current <= 0)
                Die();
        }

        private void Die()
        {
            health.HealthChanged -= HealthChanged;
            OnDying?.Invoke();
            _progressService.AddXp(_enemyData.xpReward);
            Destroy(gameObject);
        }
        
    }
}