using System.Collections.Generic;
using System.Linq;
using CodeBase.Common;
using CodeBase.Data;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Audio;
using CodeBase.Infrastructure.Services.CritDamage;
using CodeBase.Infrastructure.Services.Perk;
using CodeBase.Infrastructure.Services.Vfx;
using CodeBase.StaticData.Vfx;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Logic;
using CodeBase.StaticData.Perk;
using UnityEngine;

namespace CodeBase.Weapon
{
    public class MeleeWeapon : WeaponBase, IAttackColliderHandler, ISavedProgress
    {
        private IAudioService _audioService;
        private IPerkService _perkService;
        private IHealth _ownerHealth;
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
        private readonly List<Collider2D> _hitBuffer = new();
        private float _damage;
        private ICritDamageService _critDamageService;
        private int _critChance;
        private float _critRate;

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

        public void Construct(int damage, float cooldown, float radius, float attackAngle, int critChance,
            float critRate, ICritDamageService critDamageService)
        {
            _damage = damage;
            _cooldown = cooldown;
            _radius = radius;
            _attackAngle = attackAngle;
            _critChance = critChance;
            _critRate = critRate;
            _critDamageService = critDamageService;

            // Awake (де колайдер створюється з типовими значеннями) спрацьовує раніше за Construct,
            // тому перебудовуємо сектор уже зі справжніми радіусом і кутом зброї.
            GenerateSectorCollider();
        }

        private void Awake()
        {
            _audioService = AllServices.Container.Single<IAudioService>();
            _perkService = AllServices.Container.Single<IPerkService>();
            if (_perkService != null)
                _perkService.Changed += GenerateSectorCollider;
            _ownerHealth = GetComponentInParent<IHealth>();
            _animator = GetComponentInParent<WeaponAnimator>();
            if (_animator?.AnimationEvents != null)
            {
                _animator.AnimationEvents.OnCustomEvent -= OnAnimationEvent;
                _animator.AnimationEvents.OnCustomEvent += OnAnimationEvent;
            }
            _attackOrigin = transform.root.GetComponentInChildren<AttackColliderTrigger>().transform;
            var layerMask = PhysicsLayers.HittableMask;

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
            _audioService?.PlaySfx(AudioClipId.SwordSwing);
            _animator?.PlayAttack();
            _isAttacking = true;
        }

        private float ApllyCritDamage() =>
            _critDamageService.GetCritDamage(
                PerkDamage.WithBerserk(Damage, _perkService, _ownerHealth), _critChance, _critRate);

        private void OnAttackHit()
        {
            if (!_isAttacking) return;
            // Копія в багаторазовий буфер: смерть цілі під час удару змінює _triggeredTargets.
            _hitBuffer.Clear();
            _hitBuffer.AddRange(_triggeredTargets);

            bool hitAnyone = false;

            foreach (Collider2D target in _hitBuffer)
            {
                if (target == null) continue;

                IHealth health = target.GetComponentInParent<IHealth>();
                if (health == null || health.Current <= 0) continue;

                health.TakeDamage(ApllyCritDamage());
                hitAnyone = true;
            }

            _hitBuffer.Clear();

            if (hitAnyone)
                ApplyVampirism();
        }

        /// <summary>Перк Vampirism: замах, що влучив хоча б в одного ворога, лікує власника.</summary>
        private void ApplyVampirism()
        {
            if (_perkService == null || _ownerHealth == null) return;

            float heal = _perkService.GetValue(PerkId.Vampirism);
            if (heal <= 0f || _ownerHealth.Current <= 0f || _ownerHealth.Current >= _ownerHealth.Max) return;

            _ownerHealth.Current = Mathf.Min(_ownerHealth.Current + heal, _ownerHealth.Max);

            IVfxService vfx = AllServices.Container.Single<IVfxService>();
            if (vfx != null)
                vfx.PlayFollowing(VfxId.VampirismHeal, transform.root);
        }

        /// <summary>Кут атаки з урахуванням перку Wide Swing, не більше повного кола.</summary>
        private float EffectiveAttackAngle()
        {
            float bonus = _perkService != null ? _perkService.GetValue(PerkId.WideSwing) : 0f;
            return Mathf.Clamp(_attackAngle + bonus, 1f, 360f);
        }

        private void OnAttackEnded()
        {
            _currentCooldown = Cooldown;
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
                case AnimationEventNames.Hit:       OnAttackHit();   break;
                case AnimationEventNames.AttackEnd: OnAttackEnded(); break;
            }
        }

        private void OnDestroy()
        {
            if (_animator?.AnimationEvents != null)
                _animator.AnimationEvents.OnCustomEvent -= OnAnimationEvent;

            if (_perkService != null)
                _perkService.Changed -= GenerateSectorCollider;
        }

        private void GenerateSectorCollider()
        {
            if (_attackCollider == null) return;

            float angle = EffectiveAttackAngle();
            bool fullCircle = angle >= 360f;

            // Більший кут — більше сегментів, щоб дуга лишалась гладкою.
            int segments = Mathf.Max(16, Mathf.CeilToInt(angle / 360f * 32f));
            float halfAngle = angle * 0.5f;
            float angleStep = angle / segments;

            // Сектор — це "віяло" від центру. Повне коло будуємо без центральної точки
            // і без дубля останньої точки, інакше полігон вироджується.
            var points = new List<Vector2>(segments + 2);
            if (!fullCircle)
                points.Add(Vector2.zero);

            int lastIndex = fullCircle ? segments - 1 : segments;
            for (int i = 0; i <= lastIndex; i++)
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