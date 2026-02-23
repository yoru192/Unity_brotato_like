using System.Collections.Generic;
using System.Linq;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Weapon
{
    public class MeleeWeapon : WeaponBase, IAttackColliderHandler, ISavedProgress
    {
        private MeleeWeaponState _state;
        private Transform _attackOrigin;
        private float _attackAngle = 90f;
        private float _radius = 1.5f;

        private WeaponAnimator _animator;
        private float _cooldown;
        private float _currentCooldown;
        private bool _isAttacking;

        private PolygonCollider2D _attackCollider;
        private readonly List<Collider2D> _triggeredTargets = new();
        private float _damage;
        
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
            get => _state?.weaponCooldown ?? _cooldown;
            set
            {
                if(_state != null && _state.weaponCooldown != value)
                    _state.weaponCooldown = value;
            }
        }

        public void Construct(int damage, float cooldown, float radius, float attackAngle)
        {
            _damage = damage;
            _cooldown = cooldown;
            _radius = radius;
            _attackAngle = attackAngle;
        }

        private void Awake()
        {
            _animator = GetComponentInParent<WeaponAnimator>();
            if (_animator?.AnimationEvents != null)
            {
                _animator.AnimationEvents.OnCustomEvent -= OnAnimationEvent;
                _animator.AnimationEvents.OnCustomEvent += OnAnimationEvent;
            }
            _attackOrigin = transform.root.GetComponentInChildren<AttackColliderTrigger>().transform;
            var layerMask = 1 << LayerMask.NameToLayer("Hittable");

            _attackCollider = _attackOrigin.gameObject.AddComponent<PolygonCollider2D>();
            _attackCollider.isTrigger = true;

            var trigger = _attackOrigin.gameObject.AddComponent<AttackColliderTrigger>();
            trigger.Initialize(this, layerMask);

            GenerateSectorCollider();
        }

        private void Update()
        {
            UpdateCooldown();

            if (CanAttack() && _triggeredTargets.Count > 0)
                StartAttack();
        }

        public void OnTargetEnter(Collider2D other)
        {
            if (!_triggeredTargets.Contains(other))
                _triggeredTargets.Add(other);
        }

        public void OnTargetExited(Collider2D other) =>
            _triggeredTargets.Remove(other);
        

        private void StartAttack()
        {
            _animator?.PlayAttack();
            _isAttacking = true;
        }

        private void OnAttackHit()
        {
            if (!_isAttacking) return;

            foreach (var target in _triggeredTargets.ToList())
                target.GetComponentInParent<IHealth>()?.TakeDamage(Damage);
        }

        private void OnAttackEnded()
        {
            _currentCooldown = _cooldown;
            _isAttacking = false;
        }
        

        private void UpdateCooldown()
        {
            if (_currentCooldown > 0f)
                _currentCooldown -= Time.deltaTime;
        }

        private bool CanAttack() => !_isAttacking && _currentCooldown <= 0f;

        private void OnAnimationEvent(string eventName)
        {
            switch (eventName)
            {
                case "Hit":        OnAttackHit();   break;
                case "AttackEnd":  OnAttackEnded(); break;
            }
        }

        private void OnDestroy()
        {
            if (_animator?.AnimationEvents != null)
                _animator.AnimationEvents.OnCustomEvent -= OnAnimationEvent;
        }

        private void GenerateSectorCollider()
        {
            if (_attackCollider == null) return;

            int segments = 16;
            float halfAngle = _attackAngle * 0.5f;
            float angleStep = _attackAngle / segments;

            var points = new List<Vector2> { Vector2.zero };

            for (int i = 0; i <= segments; i++)
            {
                float currentAngle = (-halfAngle + angleStep * i) * Mathf.Deg2Rad;
                points.Add(new Vector2(
                    Mathf.Cos(currentAngle) * _radius,
                    Mathf.Sin(currentAngle) * _radius
                ));
            }

            _attackCollider.points = points.ToArray();
        }

        public void LoadProgress(PlayerProgress progress)
        {
            _state = progress.playerState.MeleeWeaponState;
            if (_state.weaponDamage == 0 && _damage > 0)
                _state.weaponDamage = _damage;
            if (_state.weaponCooldown == 0 && _cooldown > 0)
                _state.weaponCooldown = _cooldown;
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            progress.playerState.MeleeWeaponState.weaponDamage = Damage;
            progress.playerState.MeleeWeaponState.weaponCooldown = Cooldown;
        }
    }
}