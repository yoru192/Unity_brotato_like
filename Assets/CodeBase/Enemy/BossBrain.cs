using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.FantasyMonsters.Common.Scripts;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.HitFeel;
using CodeBase.Infrastructure.Services.Vfx;
using CodeBase.Logic;
using CodeBase.Player;
using CodeBase.StaticData.Enemy;
using CodeBase.StaticData.Vfx;
using CodeBase.Weapon.ArrowWeapon;
using Pathfinding;
using UnityEngine;

namespace CodeBase.Enemy
{
    /// <summary>
    /// Поведінка боса: переслідує гравця і по черзі обирає атаку. Кожна атака йде циклом
    /// попередження (стоїть і світиться) → удар → відновлення (безпечне вікно). На phase2HealthFraction
    /// бос ричить (невразливий), після чого б'є частіше й сильніше. Цифри — у BossStaticData.
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
            PhaseShift,
        }

        private enum AttackKind
        {
            Spit,
            Brood,
            Eggs,
        }

        private struct FallingEgg
        {
            public Vector2 Position;
            public float Timer;
            public bool Hatches;
            public GroundTelegraph Warning;
        }

        private static readonly AttackKind[] AttackRotation = { AttackKind.Spit, AttackKind.Brood, AttackKind.Eggs };

        [Tooltip("Префаб кола-попередження для яєць (пул Projectile).")]
        [SerializeField] private GroundTelegraph _eggWarningPrefab;

        private BossStaticData _data;
        private Transform _player;
        private PlayerHealth _playerHealth;
        private IGameFactory _gameFactory;
        private IVfxService _vfx;
        private IHitFeelService _hitFeel;

        private EnemyHealth _health;
        private EnemyDeath _death;
        private EnemyMover _mover;
        private EnemyHitFeedback _hitFeedback;
        private IAstarAI _ai;
        private Monster _monster;
        private ProjectileLauncher _launcher;

        private readonly List<EnemyHealth> _minions = new List<EnemyHealth>();
        private readonly List<FallingEgg> _eggs = new List<FallingEgg>();

        private Phase _phase;
        private int _rotationIndex;
        private float _timer;
        private bool _isDead;
        private bool _isEnraged;
        private Color _baseTint = Color.white;

        public BossStaticData Data => _data;

        public void Construct(BossStaticData data, Transform player)
        {
            _data = data;
            _player = player;
            _playerHealth = player != null ? player.GetComponent<PlayerHealth>() : null;
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
            _vfx = AllServices.Container.Single<IVfxService>();
            _hitFeel = AllServices.Container.Single<IHitFeelService>();
        }

        private void OnEnable()
        {
            _death.OnDying += OnDying;
            _health.HealthChanged += OnHealthChanged;
        }

        private void OnDisable()
        {
            _death.OnDying -= OnDying;
            _health.HealthChanged -= OnHealthChanged;
        }

        public void OnSpawn()
        {
            ResetState();
            if (_data != null && _hitFeel != null)
                _hitFeel.Shake(_data.introShakeAmplitude, _data.introShakeDuration);
        }

        public void OnDespawn()
        {
            ClearEggs();
            _minions.Clear();
            _isDead = true;
            _health.IsInvulnerable = false;
        }

        private void ResetState()
        {
            _isDead = false;
            _isEnraged = false;
            _baseTint = Color.white;
            _rotationIndex = 0;
            _minions.Clear();
            ClearEggs();
            if (_health != null)
                _health.IsInvulnerable = false;

            EnterChase(_data != null ? _data.attackInterval : 0f);

            if (_data != null && _ai != null)
                _ai.maxSpeed = _data.moveSpeed;
        }

        private void Update()
        {
            if (_isDead || _data == null || _player == null) return;

            PruneMinions();
            UpdateEggs();
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
                        EnterChase(Tempo(_data.attackInterval));
                    break;

                case Phase.PhaseShift:
                    if (_timer <= 0f)
                    {
                        _health.IsInvulnerable = false;
                        EnterChase(Tempo(_data.attackInterval) * 0.5f);
                    }
                    break;
            }
        }

        private float Tempo(float seconds) => _isEnraged ? seconds * _data.phase2TempoMultiplier : seconds;

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
            SetTint(_baseTint);
        }

        private void EnterTelegraph()
        {
            _phase = Phase.Telegraph;
            _timer = Tempo(_data.telegraphDuration);
            SetHolding(true);
            if (_monster != null)
                _monster.SetState(MonsterState.Ready);
        }

        private void ExecuteAttack()
        {
            SetTint(_baseTint);
            if (_monster != null)
                _monster.Attack();

            switch (PickAttack())
            {
                case AttackKind.Spit:
                    Spit();
                    break;
                case AttackKind.Brood:
                    Summon();
                    break;
                case AttackKind.Eggs:
                    DropEggs();
                    break;
            }

            _phase = Phase.Recover;
            _timer = Tempo(_data.recoverDuration);
        }

        // Атаки йдуть по колу; коли міньйонів уже забагато, замість виводку — плювок.
        private AttackKind PickAttack()
        {
            AttackKind attack = AttackRotation[_rotationIndex];
            _rotationIndex = (_rotationIndex + 1) % AttackRotation.Length;

            if (attack == AttackKind.Brood && _minions.Count >= _data.maxAliveMinions)
                return AttackKind.Spit;
            return attack;
        }

        // ---------- Phase 2 ----------

        private void OnHealthChanged()
        {
            if (_isDead || _isEnraged || _data == null || _health.Max <= 0f || _health.Current <= 0f) return;

            if (_health.Current / _health.Max <= _data.phase2HealthFraction)
                EnterPhaseShift();
        }

        private void EnterPhaseShift()
        {
            _isEnraged = true;
            _baseTint = _data.phase2Tint;
            _phase = Phase.PhaseShift;
            _timer = _data.phaseShiftDuration;
            _health.IsInvulnerable = true;

            SetHolding(true);
            SetTint(_baseTint);
            if (_monster != null)
                _monster.Attack();

            if (_vfx != null)
                _vfx.Play(VfxId.BossRoar, transform.position);
            if (_hitFeel != null)
                _hitFeel.Shake(_data.introShakeAmplitude, _data.phaseShiftDuration * 0.5f);
        }

        // ---------- Attacks ----------

        private void Spit()
        {
            if (_launcher == null) return;

            Vector3 toPlayer = _player.position - transform.position;
            int count = Mathf.Max(1, _isEnraged ? _data.phase2SpitProjectiles : _data.spitProjectiles);
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

        private void DropEggs()
        {
            int count = Mathf.Max(1, _isEnraged ? _data.phase2EggCount : _data.eggCount);
            float warning = Tempo(_data.eggWarningDuration);
            Vector2 center = _player.position;

            for (int i = 0; i < count; i++)
            {
                // Перше яйце — точно в гравця, щоб атаку не можна було ігнорувати.
                Vector2 position = i == 0 ? center : center + UnityEngine.Random.insideUnitCircle * _data.eggScatterRadius;

                GroundTelegraph warningCircle = null;
                if (_eggWarningPrefab != null)
                {
                    warningCircle = ObjectPoolManager.SpawnObject(_eggWarningPrefab, position, Quaternion.identity,
                        ObjectPoolManager.PoolType.Projectile);
                    warningCircle.Show(_data.eggImpactRadius, warning, _data.eggWarningColor);
                }

                _eggs.Add(new FallingEgg
                {
                    Position = position,
                    Timer = warning,
                    Hatches = i < _data.eggsThatHatch,
                    Warning = warningCircle,
                });
            }
        }

        // Яйця падають незалежно від фази боса (поки він живий).
        private void UpdateEggs()
        {
            for (int i = _eggs.Count - 1; i >= 0; i--)
            {
                FallingEgg egg = _eggs[i];
                egg.Timer -= Time.deltaTime;
                if (egg.Timer > 0f)
                {
                    _eggs[i] = egg;
                    continue;
                }

                _eggs.RemoveAt(i);
                ImpactEgg(egg);
            }
        }

        private void ImpactEgg(FallingEgg egg)
        {
            if (_vfx != null)
                _vfx.Play(VfxId.EggImpact, egg.Position);

            float radius = _data.eggImpactRadius;
            if (_playerHealth != null && ((Vector2)_player.position - egg.Position).sqrMagnitude <= radius * radius)
                _playerHealth.TakeDamage(_data.eggDamage);

            if (egg.Hatches && _minions.Count < _data.maxAliveMinions)
                _ = SummonMinion(egg.Position);
        }

        private void ClearEggs()
        {
            foreach (FallingEgg egg in _eggs)
                if (egg.Warning != null)
                    egg.Warning.Hide();
            _eggs.Clear();
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

        // Зі смертю королеви гине і її виводок, а яйця, що ще падають, зникають.
        private void OnDying()
        {
            _isDead = true;
            _health.IsInvulnerable = false;
            SetTint(Color.white);
            ClearEggs();

            for (int i = _minions.Count - 1; i >= 0; i--)
            {
                EnemyHealth minion = _minions[i];
                if (minion != null && minion.gameObject.activeInHierarchy && minion.Current > 0f)
                    minion.TakeStatusDamage(minion.Current);
            }
            _minions.Clear();

            if (_vfx != null)
                _vfx.Play(VfxId.BossDeath, transform.position);
            if (_hitFeel != null && _data != null)
            {
                _hitFeel.HitStop(_data.deathHitStop);
                _hitFeel.Shake(_data.deathShakeAmplitude, _data.deathShakeDuration);
            }
        }

        // ---------- Visuals ----------

        private void PulseTelegraphTint()
        {
            float duration = Tempo(_data.telegraphDuration);
            float progress = 1f - Mathf.Clamp01(_timer / duration);
            float pulse = 0.5f + 0.5f * Mathf.Sin(progress * Mathf.PI * 6f);
            SetTint(Color.Lerp(_baseTint, _data.telegraphTint, pulse));
        }

        private void SetTint(Color tint)
        {
            if (_hitFeedback == null)
                _hitFeedback = GetComponent<EnemyHitFeedback>();
            if (_hitFeedback != null)
                _hitFeedback.SetTelegraphTint(tint);
        }

        // Під час попередження, відновлення й ричання бос стоїть, а EnemyMover не перебиває анімацію.
        private void SetHolding(bool holding)
        {
            if (_ai != null)
                _ai.isStopped = holding;
            if (_mover != null)
                _mover.SetAttacking(holding);
        }
    }
}
