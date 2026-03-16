using System;
using System.Collections;
using Assets.FantasyMonsters.Common.Scripts;
using CodeBase.Infrastructure.Services.Balance;
using CodeBase.Infrastructure.Services.ProgressService;
using CodeBase.Logic;
using CodeBase.StaticData.Enemy;
using Pathfinding;
using UnityEngine;

namespace CodeBase.Enemy
{
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyDeath : MonoBehaviour
    {
        public EnemyHealth health;
        public event Action OnDying;

        private IProgressService _progressService;
        private EnemyStaticData _enemyData;
        private IBalanceService _balanceService;
        private Monster _monster;
        
        public bool IsConstructed { get; set; }

        public void Construct(IBalanceService balanceService, IProgressService progressService, EnemyStaticData enemyData)
        {
            _balanceService = balanceService;
            _progressService = progressService;
            _enemyData = enemyData;
            
            _monster = GetComponentInChildren<Monster>();
            health.HealthChanged += HealthChanged;
        }
        
        private void OnEnable()
        {
            if (IsConstructed)
            {
                _monster = GetComponentInChildren<Monster>();
                health.HealthChanged += HealthChanged;
            }
        }

        private void OnDisable()
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
            
           if (TryGetComponent<IAstarAI>(out var ai)) ai.isStopped = true;
           if (TryGetComponent<EnemyAttack>(out var attack)) attack.enabled = false;
           if (TryGetComponent<EnemyMover>(out var mover)) mover.enabled = false;
            
            _monster.Die();
            StartCoroutine(DestroyAfterAnimation());
        }

        private IEnumerator DestroyAfterAnimation()
        {
            yield return null;

            var animator = _monster.Animator;
            while (true)
            {
                var state = animator.GetCurrentAnimatorStateInfo(0);
                if (state.IsName("Death") && state.normalizedTime >= 1f)
                    break;
                yield return null;
            }
            
            ObjectPoolManager.ReturnObjectToPool(gameObject, ObjectPoolManager.PoolType.Enemy);
        }
    }
}
