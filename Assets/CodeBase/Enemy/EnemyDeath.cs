using System;
using System.Collections;
using Assets.FantasyMonsters.Common.Scripts;
using CodeBase.Infrastructure.Services.Balance;
using CodeBase.Infrastructure.Services.ProgressService;
using CodeBase.Logic;
using CodeBase.Loot;
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

        [SerializeField] private float xpSpawnRadius = 2.5f;

        private IProgressService _progressService;
        private EnemyStaticData _enemyData;
        private IBalanceService _balanceService;
        private Monster _monster;
        private XpOrbSet _xpOrbSet;
        private Transform _playerTransform;

        public bool IsConstructed { get; set; }

        public void Construct(IBalanceService balanceService, IProgressService progressService, EnemyStaticData enemyData, XpOrbSet xpOrbSet, Transform playerTransform)
        {
            _balanceService = balanceService;
            _progressService = progressService;
            _enemyData = enemyData;
            _xpOrbSet = xpOrbSet;
            _playerTransform = playerTransform;

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

            SpawnXpOrbs();
            _balanceService.AddBalance(_enemyData.balanceReward);

            if (TryGetComponent<IAstarAI>(out var ai)) ai.isStopped = true;
            if (TryGetComponent<EnemyAttack>(out var attack)) attack.enabled = false;
            if (TryGetComponent<EnemyMover>(out var mover)) mover.enabled = false;

            _monster.Die();
            StartCoroutine(DestroyAfterAnimation());
        }

        private void SpawnXpOrbs()
        {
            int remaining = _enemyData.xpReward;

            (int value, XpOrb prefab)[] denominations =
            {
                (XpOrbSet.LargeValue, _xpOrbSet.Large),
                (XpOrbSet.MediumValue, _xpOrbSet.Medium),
                (XpOrbSet.SmallValue, _xpOrbSet.Small),
            };

            foreach (var (value, prefab) in denominations)
            {
                if (prefab == null) continue;
                int count = remaining / value;
                remaining %= value;
                for (int i = 0; i < count; i++)
                    SpawnOrb(prefab, value);
            }
        }

        private void SpawnOrb(XpOrb prefab, int value)
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * xpSpawnRadius;
            Vector2 pos = (Vector2)transform.position + offset;
            XpOrb orb = ObjectPoolManager.SpawnObject(prefab, pos, Quaternion.identity, ObjectPoolManager.PoolType.XpOrb);
            orb.Construct(value, _progressService, _playerTransform);
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
