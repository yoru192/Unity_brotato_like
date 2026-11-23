using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.FantasyMonsters.Common.Scripts;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Logic;
using CodeBase.StaticData.Enemy;
using CodeBase.Weapon.ArrowWeapon;
using Pathfinding;
using UnityEngine;

namespace CodeBase.Enemy
{
    /// <summary>
    /// Поведінка боса: переслідує гравця і по черзі обирає атаку. Кожна атака йде циклом
    /// попередження (стоїть і світиться) → удар → відновлення (безпечне вікно). Цифри — у BossStaticData.
    /// Додається на префаб боса; Construct викликає фабрика.
    /// </summary>
    [RequireComponent(typeof(EnemyHealth))]
    [RequireComponent(typeof(EnemyDeath))]
    public class BossBrain : MonoBehaviour, IPoolable
    {
        private enum Phase
        {
            Chase,
            Telegraph,
            Recover,
        }

        private enum AttackKind
        {
            Spit,
            Brood,
        }

        private BossStaticData _data;
        private Transform _player;
        private IGameFactory _gameFactory;

        private EnemyHealth _health;
        private EnemyDeath _death;
        private EnemyMover _mover;
        private EnemyHitFeedback _hitFeedback;
        private IAstarAI _ai;
        private Monster _monster;
        private ProjectileLauncher _launcher;

        private readonly List<EnemyHealth> _minions = new List<EnemyHealth>();

        private Phase _phase;
        private AttackKind _nextAttack;
        private float _timer;
        private bool _isDead;

        public BossStaticData Data => _data;

        public void Construct(BossStaticData data, Transform player)
        {
            _data = data;
            _player = player;
            ResetState();
        }

        private void Awake()
        {
            _health = GetComponent<EnemyHealth>();
            _death = GetComponent<EnemyDeath>();
            _mover = GetComponent<EnemyMover>();
            _ai = GetComponent<IAstarAI>();
            _launcher = GetComponent<ProjectileLauncher>();
            _monster = GetComponentInChildren<Monster>();
            _gameFactory = AllServices.Container.Single<IGameFactory>();
        }

        private void OnEnable()
        {
            _death.OnDying += OnDying;
        }

        private void OnDisable()
        {
            _death.OnDying -= OnDying;
        }

        public void OnSpawn() => ResetState();

        public void OnDespawn()
        {
            _minions.Clear();
            _isDead = true;
        }

        private void ResetState()
        {
            _isDead = false;
            _minions.Clear();
            EnterChase(_data != null ? _data.attackInterval : 0f);
            _nextAttack = AttackKind.Spit;

            if (_data != null && _ai != null)
                _ai.maxSpeed = _data.moveSpeed;
        }

        private void Update()
        {
            if (_isDead || _data == null || _player == null) return;

            PruneMinions();
            _timer -= Time.deltaTime;

            switch (_phase)
            {
                case Phase.Chase:
                    if (_timer <= 0f && IsPlayerInRange())
                        EnterTelegraph();
                    break;

                case Phase.Telegraph:
                    PulseTelegraphTint();
                    if (_timer <= 0f)
                        ExecuteAttack();
                    break;

                case Phase.Recover:
                    if (_timer <= 0f)
                        EnterChase(_data.attackInterval);
                    break;
            }
        }

        private bool IsPlayerInRange()
        {
            float range = _data.engageRange;
            return ((Vector2)(_player.position - transform.position)).sqrMagnitude <= range * range;
        }

        private void EnterChase(float delay)
        {
            _phase = Phase.Chase;
            _timer = delay;
            SetHolding(false);
            SetTint(Color.white);
        }

        private void EnterTelegraph()
        {
            _phase = Phase.Telegraph;
            _timer = _data.telegraphDuration;
            SetHolding(true);
            if (_monster != null)
                _monster.SetState(MonsterState.Ready);
        }

        private void ExecuteAttack()
        {
            SetTint(Color.white);
            if (_monster != null)
                _monster.Attack();

            AttackKind attack = PickAttack();
            if (attack == AttackKind.Spit)
                Spit();
            else
                Summon();

            _nextAttack = attack == AttackKind.Spit ? AttackKind.Brood : AttackKind.Spit;
            _phase = Phase.Recover;
            _timer = _data.recoverDuration;
        }

        // Атаки чергуються; коли міньйонів уже забагато, замість виводку — плювок.
        private AttackKind PickAttack()
        {
            if (_nextAttack == AttackKind.Brood && _minions.Count >= _data.maxAliveMinions)
                return AttackKind.Spit;
            return _nextAttack;
        }

        private void Spit()
        {
            if (_launcher == null) return;

            Vector3 toPlayer = _player.position - transform.position;
            int count = Mathf.Max(1, _data.spitProjectiles);
            for (int i = 0; i < count; i++)
            {
                float t = count == 1 ? 0.5f : i / (float)(count - 1);
                float angle = Mathf.Lerp(-_data.spitSpreadAngle, _data.spitSpreadAngle, t) * 0.5f;
                Vector3 aimOffset = Quaternion.Euler(0f, 0f, angle) * toPlayer - toPlayer;
                _launcher.LaunchWithOffset(_player, _data.spitProjectileSpeed, _data.spitDamage, aimOffset);
            }
        }

        private void Summon()
        {
            int count = Mathf.Min(_data.summonCount, _data.maxAliveMinions - _minions.Count);
            float step = 360f / Mathf.Max(1, count);
            float start = UnityEngine.Random.Range(0f, 360f);

            for (int i = 0; i < count; i++)
            {
                Vector2 direction = Quaternion.Euler(0f, 0f, start + step * i) * Vector2.right;
                _ = SummonMinion((Vector2)transform.position + direction * _data.summonRadius);
            }
        }

        private async Task SummonMinion(Vector2 position)
        {
            try
            {
                GameObject minion = await _gameFactory.CreateEnemy(_data.minionTypeId, position);

                // Бос міг загинути або піти в пул, поки міньйон вантажився.
                if (minion == null || this == null || _isDead) return;

                if (minion.TryGetComponent(out EnemyHealth minionHealth))
                    _minions.Add(minionHealth);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void PruneMinions()
        {
            for (int i = _minions.Count - 1; i >= 0; i--)
            {
                EnemyHealth minion = _minions[i];
                if (minion == null || !minion.gameObject.activeInHierarchy || minion.Current <= 0f)
                    _minions.RemoveAt(i);
            }
        }

        // Зі смертю королеви гине і її виводок, щоб після перемоги ніхто не кусав гравця.
        private void OnDying()
        {
            _isDead = true;
            SetTint(Color.white);

            for (int i = _minions.Count - 1; i >= 0; i--)
            {
                EnemyHealth minion = _minions[i];
                if (minion != null && minion.gameObject.activeInHierarchy && minion.Current > 0f)
                    minion.TakeStatusDamage(minion.Current);
            }
            _minions.Clear();
        }

        private void PulseTelegraphTint()
        {
            float progress = 1f - Mathf.Clamp01(_timer / _data.telegraphDuration);
            float pulse = 0.5f + 0.5f * Mathf.Sin(progress * Mathf.PI * 6f);
            SetTint(Color.Lerp(Color.white, _data.telegraphTint, pulse));
        }

        private void SetTint(Color tint)
        {
            if (_hitFeedback == null)
                _hitFeedback = GetComponent<EnemyHitFeedback>();
            if (_hitFeedback != null)
                _hitFeedback.SetTelegraphTint(tint);
        }

        // Під час попередження й відновлення бос стоїть, а EnemyMover не перебиває анімацію.
        private void SetHolding(bool holding)
        {
            if (_ai != null)
                _ai.isStopped = holding;
            if (_mover != null)
                _mover.SetAttacking(holding);
        }
    }
}
