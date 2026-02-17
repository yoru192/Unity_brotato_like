using System.Collections.Generic;
using System.Linq;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Weapon
{
    public class WeaponAttack : MonoBehaviour, ISavedProgress
    {
        [SerializeField] private Transform _attackOrigin;
        [SerializeField] private float _attackAngle = 90f;
        [SerializeField] private PolygonCollider2D _attackCollider;
        
        private float _attackCooldown;
        private float _radius;
        private int _damage;
        private State _state;
        private WeaponAnimator _animator;
        private float _currentCooldown;
        private int _layerMask;
        private bool _isAttacking;
        private List<Collider2D> _triggeredTargets = new List<Collider2D>();

        public int Damage
        {
            get => (int)(_state?.weaponDamage ?? _damage);
            set
            {
                if(_state != null && _state.weaponDamage != value)
                    _state.weaponDamage = value;
            }
        }

        public float Cooldown
        {
            get => _state?.weaponCooldown ?? _attackCooldown;
            set
            {
                if(_state != null && _state.weaponCooldown != value)
                    _state.weaponCooldown = value;
            }
        }

        public void Construct(WeaponAnimator animator, float radius, int damage, float cooldown)
        {
            if (animator != null)
                _animator = animator;
            _radius = radius;
            _damage = damage;
            _attackCooldown = cooldown;
            
            GenerateSectorCollider();
            
            if (_animator != null && _animator.AnimationEvents != null)
            {
                _animator.AnimationEvents.OnCustomEvent -= OnAnimationEvent;
                _animator.AnimationEvents.OnCustomEvent += OnAnimationEvent;
            }
        }

        private void Awake()
        {
            _layerMask = 1 << LayerMask.NameToLayer("Hittable");
            _animator = GetComponentInParent<WeaponAnimator>();
            
            if (_attackOrigin != null)
            {
                _attackCollider = _attackOrigin.GetComponent<PolygonCollider2D>();
                if (_attackCollider == null)
                {
                    _attackCollider = _attackOrigin.gameObject.AddComponent<PolygonCollider2D>();
                    _attackCollider.isTrigger = true;
                }
                
                var triggerHandler = _attackOrigin.GetComponent<AttackColliderTrigger>();
                if (triggerHandler == null)
                {
                    triggerHandler = _attackOrigin.gameObject.AddComponent<AttackColliderTrigger>();
                }
                triggerHandler.Initialize(this, _layerMask);
            }
            
            _attackCollider.enabled = true;
            GenerateSectorCollider();

            if (_animator != null && _animator.AnimationEvents != null)
            {
                _animator.AnimationEvents.OnCustomEvent += OnAnimationEvent;
            }
        }

        private void GenerateSectorCollider()
        {
            if (_attackCollider == null) return;
            
            int segments = 16;
            float halfAngle = _attackAngle * 0.5f;
            float angleStep = _attackAngle / segments;
            
            List<Vector2> points = new List<Vector2>();
            points.Add(Vector2.zero);
            
            for (int i = 0; i <= segments; i++)
            {
                float currentAngle = (-halfAngle + angleStep * i) * Mathf.Deg2Rad;
                float x = Mathf.Cos(currentAngle) * _radius;
                float y = Mathf.Sin(currentAngle) * _radius;
                points.Add(new Vector2(x, y));
            }
            
            _attackCollider.points = points.ToArray();
        }

        private void Update()
        {
            UpdateCooldown();
            if (CanAttack() && HasTarget())
                StartAttack();
        }

        private void StartAttack()
        {
            _animator.PlayAttack();
            _isAttacking = true;
        }

        public void OnAttack()
        {
            if (!_isAttacking) return;
            
            foreach (var target in _triggeredTargets.ToList())
            {
                if (target != null && target.gameObject.activeInHierarchy)
                {
                    Debug.Log($"<color=green>HIT! Dealing {Damage} damage to {target.name}</color>");
                    var health = target.GetComponentInParent<IHealth>();
                    if (health != null)
                    {
                        health.TakeDamage(Damage);
                    }
                }
            }
        }

        public void OnAttackEnded()
        {
            _currentCooldown = Cooldown;
            _isAttacking = false;
            _triggeredTargets.Clear();
        }

        private void UpdateCooldown()
        {
            if (!CooldownIsUp())
                _currentCooldown -= Time.deltaTime;
        }

        private bool CooldownIsUp() => _currentCooldown <= 0f;
        private bool CanAttack() => !_isAttacking && CooldownIsUp();

        private bool HasTarget()
        {
            return _triggeredTargets.Count > 0;
        }
        
        public void OnTargetStayed(Collider2D other)
        {
            if (!_triggeredTargets.Contains(other))
            {
                _triggeredTargets.Add(other);
                Debug.Log($"Target entered sector: {other.name}");
            }
        }
        

        public void OnTargetExited(Collider2D other)
        {
            _triggeredTargets.Remove(other);
        }
        
        private Vector2 StartPoint()
        {
            if (_attackOrigin != null)
                return _attackOrigin.position;
            
            float direction = Mathf.Sign(transform.lossyScale.x);
            return (Vector2)transform.position + 
                   Vector2.up * 0.5f + 
                   Vector2.right * _radius * direction;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            Vector3 currentCenter = StartPoint();
            float attackDirection = Mathf.Sign(transform.lossyScale.x);
            
            Gizmos.color = _isAttacking ? Color.green : Color.red;
            DrawSector(currentCenter, _radius, attackDirection, _attackAngle);
            
            foreach (var target in _triggeredTargets)
            {
                if (target != null)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(currentCenter, target.transform.position);
                    Gizmos.DrawWireSphere(target.transform.position, 0.2f);
                }
            }
        }

        private void DrawSector(Vector3 center, float radius, float direction, float angle)
        {
            int segments = 32;
            float halfAngle = angle * 0.5f;
            float angleStep = angle / segments;
            
            Vector3 previousPoint = center + Quaternion.Euler(0, 0, -halfAngle) * (Vector3.right * radius * direction);
            Gizmos.DrawLine(center, previousPoint);
            
            for (int i = 1; i <= segments; i++)
            {
                float currentAngle = -halfAngle + (angleStep * i);
                Vector3 newPoint = center + Quaternion.Euler(0, 0, currentAngle) * (Vector3.right * radius * direction);
                Gizmos.DrawLine(previousPoint, newPoint);
                previousPoint = newPoint;
            }
            
            Gizmos.DrawLine(previousPoint, center);
        }

        public void LoadProgress(PlayerProgress progress)
        {
            _state = progress.playerState;
            if (_state.weaponDamage == 0 && _damage > 0)
                _state.weaponDamage = _damage;
            if (_state.weaponCooldown == 0 && _attackCooldown > 0)
                _state.weaponCooldown = _attackCooldown;
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            progress.playerState.weaponDamage = Damage;
            progress.playerState.weaponCooldown = Cooldown;
        }
        
        private void OnDestroy()
        {
            if (_animator != null && _animator.AnimationEvents != null)
            {
                _animator.AnimationEvents.OnCustomEvent -= OnAnimationEvent;
            }
        }

        private void OnAnimationEvent(string eventName)
        {
            switch (eventName)
            {
                case "Hit":
                    OnAttack();
                    break;
                case "AttackEnd":
                    OnAttackEnded();
                    break;
            }
        }
    }
}
