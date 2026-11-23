using System;
using Assets.FantasyMonsters.Common.Scripts;
using CodeBase.Common;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Perk;
using CodeBase.Infrastructure.Services.Vfx;
using CodeBase.StaticData.Vfx;
using CodeBase.Logic;
using CodeBase.Player;
using CodeBase.StaticData.Perk;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class EnemyAttack : MonoBehaviour, IAttackWithCooldown, IPoolable
    {
        public event Action OnCooldownStarted;
        public event Action OnCooldownEnded;

        [Tooltip("Для анімацій без події AttackEnded (напр. Larva): атака завершується одразу після удару.")]
        [SerializeField] private bool _endAttackOnHitEvent;

        private EnemyMover _mover;
        private Monster _monster;
        private float _attackCooldown;
        private float _radius;
        private int _damage;
        private Transform _playerTransform;
        private float _currentAttackCooldown;
        private bool _isAttacking;
        private Collider2D[] _hits = new Collider2D[1];
        private int _layerMask;
        private bool _attackIsActive;
        private Collider2D _enemyCollider;
        private Collider2D _playerCollider;
        private EnemyHealth _health;
        private IPerkService _perkService;

        public void Construct(Transform playerTransform, float attackCooldown,
            float radius, int damage)
        {
            _playerTransform = playerTransform;
            _playerCollider = playerTransform != null ? playerTransform.GetComponent<Collider2D>() : null;
            _attackCooldown = attackCooldown;
            _radius = radius;
            _damage = damage;
            _currentAttackCooldown = attackCooldown;
        }

        private void Awake()
        {
            _layerMask = PhysicsLayers.PlayerMask;
            _enemyCollider = GetComponent<Collider2D>();
            _monster = GetComponentInChildren<Monster>();
            _mover = GetComponent<EnemyMover>();
            _health = GetComponent<EnemyHealth>();
            _perkService = AllServices.Container.Single<IPerkService>();
            _monster.OnEvent += HandleAnimationEvent;
        }

        /// <summary>Перк Thorns: ворог, що влучив по гравцю в ближньому бою, сам отримує урон.</summary>
        private void ApplyThorns()
        {
            if (_perkService == null || _health == null) return;

            float thorns = _perkService.GetValue(PerkId.Thorns);
            if (thorns <= 0f) return;

            _health.TakeDamage(thorns, false);

            IVfxService vfx = AllServices.Container.Single<IVfxService>();
            if (vfx != null)
                vfx.Play(VfxId.ThornsHit, transform.position);
        }

        private void HandleAnimationEvent(string eventName)
        {
            switch (eventName)
            {
                case AnimationEventNames.Attack:
                    OnAttack();
                    if (_endAttackOnHitEvent)
                        OnAttackEnded();
                    break;
                case AnimationEventNames.AttackEnded:
                    OnAttackEnded();
                    break;
            }
        }


        private void Update()
        {
            UpdateCooldown();

            if (CanAttack())
                StartAttack();
        }

        private void UpdateCooldown()
        {
            if (!CooldownIsUp())
            {
                _currentAttackCooldown -= Time.deltaTime;

                if (CooldownIsUp())
                    OnCooldownEnded?.Invoke();
            }
        }

        private void OnAttack()
        {
            if (Hit(out Collider2D hit))
            {
                hit.GetComponentInParent<PlayerHealth>().TakeDamage(_damage);
                ApplyThorns();
            }
        }

        private void OnAttackEnded()
        {
            _currentAttackCooldown = _attackCooldown;
            _isAttacking = false;
            _mover.SetAttacking(false);
            OnCooldownStarted?.Invoke();
        }

        private bool Hit(out Collider2D hit)
        {
            var hitAmount = Physics2D.OverlapCircleNonAlloc(
                transform.position, 
                _radius,
                _hits, 
                _layerMask);
            
            hit = hitAmount > 0 ? _hits[0] : null;
            return hitAmount > 0;
        }

        private bool CooldownIsUp()
        {
            return _currentAttackCooldown <= 0f;
        }

        private bool CanAttack()
        {
            if (!_isAttacking && CooldownIsUp())
            {
                if (_playerCollider != null)
                {
                    ColliderDistance2D distanceInfo = Physics2D.Distance(_enemyCollider, _playerCollider);
                    return distanceInfo.distance <= _radius;
                }
                else
                {
                    float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);
                    return distanceToPlayer <= _radius;
                }
            }
            else
            {
                return false;
            }
        }

        private void StartAttack()
        {
            _monster.Attack();
            _isAttacking = true;
            _mover.SetAttacking(true);
        }

        public void OnSpawn()
        {
            _isAttacking = false;
            _currentAttackCooldown = _attackCooldown;
        }

        public void OnDespawn() { }
        
        private void OnDrawGizmosSelected()
        {
            Collider2D enemyCol = _enemyCollider != null ? _enemyCollider : GetComponent<Collider2D>();
            
            // Радіус атаки
            Gizmos.color = Color.red;
            DrawCircle(transform.position, _radius, 30);
            
            // Показуємо колайдер ворога
            if (enemyCol != null && enemyCol is CircleCollider2D circle)
            {
                Gizmos.color = Color.cyan;
                DrawCircle(transform.position, circle.radius, 20);
            }
            
            if (_playerTransform != null)
            {
                Collider2D playerCol = _playerTransform.GetComponent<Collider2D>();
                float distance = 0;
                
                if (enemyCol != null && playerCol != null)
                {
                    // Реальна дистанція між колайдерами
                    ColliderDistance2D distanceInfo = Physics2D.Distance(enemyCol, playerCol);
                    distance = distanceInfo.distance;
                    
                    // Показуємо найближчу точку між колайдерами
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(distanceInfo.pointA, distanceInfo.pointB);
                    Gizmos.DrawWireSphere(distanceInfo.pointA, 0.1f);
                    Gizmos.DrawWireSphere(distanceInfo.pointB, 0.1f);
                }
                else
                {
                    distance = Vector2.Distance(transform.position, _playerTransform.position);
                }
                
                // Зелена = в радіусі, жовта = поза радіусом
                Gizmos.color = distance <= _radius ? Color.green : Color.yellow;
                Gizmos.DrawLine(transform.position, _playerTransform.position);
                
                #if UNITY_EDITOR
                Vector3 midPoint = (transform.position + _playerTransform.position) / 2f;
                UnityEditor.Handles.Label(midPoint, $"Collider Dist: {distance:F2}");
                #endif
            }
            
            if (_isAttacking)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, _radius * 1.1f);
            }
            
            if (!CooldownIsUp())
            {
                Gizmos.color = Color.cyan;
                float cooldownPercent = _currentAttackCooldown / _attackCooldown;
                Gizmos.DrawWireSphere(transform.position, _radius * cooldownPercent);
            }
        }

        private void DrawCircle(Vector3 center, float radius, int segments)
        {
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }
    }
}