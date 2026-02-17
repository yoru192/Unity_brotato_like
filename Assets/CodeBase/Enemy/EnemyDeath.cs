using System;
using System.Collections;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Balance;
using CodeBase.Infrastructure.Services.ProgressService;
using CodeBase.StaticData;
using CodeBase.StaticData.Enemy;
using UnityEngine;

namespace CodeBase.Enemy
{
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyDeath : MonoBehaviour
    {
        public EnemyHealth health;
        private IProgressService _progressService;
        private EnemyStaticData _enemyData;
        private IBalanceService _balanceService;
        public event Action OnDying;

        public void Construct( IBalanceService balanceService,IProgressService progressService, EnemyStaticData enemyData)
        {
            _balanceService = balanceService;
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
            _balanceService.AddBalance(_enemyData.balanceReward);
            Destroy(gameObject);
        }
        
    }
}