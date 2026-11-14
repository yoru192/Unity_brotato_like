using System.Collections.Generic;
using CodeBase.Infrastructure.Services.Audio;
using CodeBase.Infrastructure.Services.Perk;
using CodeBase.Logic;
using UnityEngine;
using CodeBase.Player.Movement;
using CodeBase.StaticData.Ability;
using CodeBase.StaticData.Perk;

namespace CodeBase.Ability.Concrete
{
    public class DashAbility : AbilityBase
    {
        // Запас до дистанції ривка, у межах якого шукаємо ворогів для прольоту крізь них.
        private const float PassThroughSearchMargin = 3f;

        // Спільний буфер для фізичних запитів: без алокацій на кожен кадр.
        private static readonly List<Collider2D> QueryBuffer = new List<Collider2D>(64);

        private readonly PlayerMovement playerMovement;
        private readonly DashAbilityData dashData;

        private bool isDashing;
        private float dashTimer;
        private Vector2 dashDirection;
        private Vector2 originalVelocity;
        private readonly IAudioService _audioService;
        private readonly IPerkService _perkService;

        // Перк Deadly Dash: кожного ворога зачіпаємо не більше одного разу за ривок.
        private readonly HashSet<IHealth> _damagedThisDash = new HashSet<IHealth>();

        // Колайдери ворогів, зіткнення з якими вимкнено на час прольоту.
        private readonly List<Collider2D> _ignoredColliders = new List<Collider2D>();

        // У ворогів кілька колайдерів на різних шарах, тому шукаємо по всіх шарах і відбираємо за IHealth.
        private readonly ContactFilter2D _solidFilter;
        private readonly Collider2D _bodyCollider;
        private readonly IHealth _ownerHealth;

        private float _dashDamage;
        private Vector2 _lastBodyCenter;

        public DashAbility(
            Transform playerTransform,
            Rigidbody2D playerRb,
            PlayerMovement playerMovement,
            DashAbilityData staticData,
            IAudioService audioService,
            IPerkService perkService)
            : base(playerTransform, playerRb, staticData)
        {
            this.playerMovement = playerMovement;
            dashData = staticData;
            _audioService = audioService;
            _perkService = perkService;
            _bodyCollider = FindBodyCollider(playerRb);
            _ownerHealth = playerTransform.GetComponentInChildren<IHealth>();

            _solidFilter = new ContactFilter2D { useTriggers = false };
        }


        protected override void OnExecute()
        {
            _audioService.PlaySfx(AudioClipId.Dash);
            // Зберігаємо поточну швидкість
            originalVelocity = playerRb.linearVelocity;

            // Визначаємо напрямок dash
            if (originalVelocity.magnitude > 0.1f)
            {
                dashDirection = originalVelocity.normalized;
            }
            else
            {
                // Якщо не рухаємося, dash у напрямку погляду
                dashDirection = new Vector2(playerTransform.localScale.x, 0f);
            }

            isDashing = true;
            dashTimer = 0f;

            _dashDamage = _perkService != null ? _perkService.GetValue(PerkId.DeadlyDash) : 0f;
            _damagedThisDash.Clear();
            _lastBodyCenter = BodyCenter();

            if (_dashDamage > 0f)
                IgnoreEnemiesAlongDash();

            // Забираємо керування рухом на час dash. Саме прапорець, а не enabled: enabled належить
            // станам гри, і ривок не має права ввімкнути керування, яке вимкнув стан (екран апгрейдів, смерть).
            playerMovement.IsMovementOverridden = true;
        }

        protected override void OnTick(float deltaTime)
        {
            if (!isDashing)
            {
                // Після ривка: повертаємо зіткнення з ворогами, щойно гравець перестав у них стояти.
                RestoreSeparatedColliders();
                return;
            }

            dashTimer += deltaTime;
            float progress = dashTimer / dashData.dashDuration;

            if (progress >= 1f)
            {
                DamageEnemiesOnPath();
                EndDash();
                return;
            }

            // Плавний dash через AnimationCurve
            float curveValue = dashData.dashCurve.Evaluate(progress);
            float dashSpeed = (dashData.dashDistance / dashData.dashDuration) * curveValue;

            playerRb.linearVelocity = dashDirection * dashSpeed;

            DamageEnemiesOnPath();
        }

        /// <summary>
        /// Перк Deadly Dash: урон усім ворогам у смузі, яку тіло гравця пройшло з минулого кадру.
        /// Смуга, а не точка — бо ривок швидкий і за кадр гравець пролітає більше за ширину ворога.
        /// </summary>
        private void DamageEnemiesOnPath()
        {
            if (_dashDamage <= 0f) return;

            Vector2 center = BodyCenter();
            Vector2 delta = center - _lastBodyCenter;
            _lastBodyCenter = center;

            float bodySize = BodySize() + dashData.dashDamageRadius * 2f;
            float travelled = delta.magnitude;
            Vector2 direction = travelled > 0.0001f ? delta / travelled : dashDirection;

            Vector2 boxCenter = center - delta * 0.5f;
            Vector2 boxSize = new Vector2(bodySize + travelled, bodySize);
            float angle = Vector2.SignedAngle(Vector2.right, direction);

            int count = Physics2D.OverlapBox(boxCenter, boxSize, angle, _solidFilter, QueryBuffer);
            for (int i = 0; i < count; i++)
            {
                IHealth health = EnemyHealthOf(QueryBuffer[i]);
                if (health == null || !_damagedThisDash.Add(health)) continue;

                health.TakeDamage(_dashDamage, false);
            }

            QueryBuffer.Clear();
        }

        /// <summary>
        /// Вимикає зіткнення тіла гравця з усіма колайдерами ворогів на шляху ривка, щоб він пролітав крізь них.
        /// Працюємо з конкретними колайдерами, а не з шаром: у ворогів колайдери лежать на різних шарах.
        /// </summary>
        private void IgnoreEnemiesAlongDash()
        {
            if (_bodyCollider == null) return;

            float radius = dashData.dashDistance + PassThroughSearchMargin;
            int count = Physics2D.OverlapCircle(BodyCenter(), radius, _solidFilter, QueryBuffer);

            for (int i = 0; i < count; i++)
            {
                Collider2D collider = QueryBuffer[i];
                if (EnemyHealthOf(collider) == null || _ignoredColliders.Contains(collider)) continue;

                Physics2D.IgnoreCollision(_bodyCollider, collider, true);
                _ignoredColliders.Add(collider);
            }

            QueryBuffer.Clear();
        }

        /// <summary>
        /// Повертає зіткнення з ворогами, з якими гравець уже не перетинається. З тими, у яких він ще стоїть,
        /// зіткнення лишається вимкненим — інакше фізика різко виштовхнула б гравця назовні.
        /// </summary>
        private void RestoreSeparatedColliders()
        {
            if (_ignoredColliders.Count == 0) return;

            for (int i = _ignoredColliders.Count - 1; i >= 0; i--)
            {
                Collider2D collider = _ignoredColliders[i];

                if (collider == null || _bodyCollider == null)
                {
                    _ignoredColliders.RemoveAt(i);
                    continue;
                }

                if (StillOverlapping(collider)) continue;

                Physics2D.IgnoreCollision(_bodyCollider, collider, false);
                _ignoredColliders.RemoveAt(i);
            }
        }

        private bool StillOverlapping(Collider2D collider)
        {
            if (!collider.enabled || !collider.gameObject.activeInHierarchy) return false;

            ColliderDistance2D distance = _bodyCollider.Distance(collider);
            return distance.isValid && distance.isOverlapped;
        }

        /// <summary>IHealth ворога, якому належить колайдер; null для гравця, стін та іншого.</summary>
        private IHealth EnemyHealthOf(Collider2D collider)
        {
            if (collider == null || collider.attachedRigidbody == playerRb) return null;

            IHealth health = collider.GetComponentInParent<IHealth>();
            return health == _ownerHealth ? null : health;
        }

        private Vector2 BodyCenter() =>
            _bodyCollider != null ? (Vector2)_bodyCollider.bounds.center : (Vector2)playerTransform.position;

        private float BodySize()
        {
            if (_bodyCollider == null) return 1f;

            Vector3 size = _bodyCollider.bounds.size;
            return Mathf.Max(size.x, size.y);
        }

        private static Collider2D FindBodyCollider(Rigidbody2D rb)
        {
            if (rb == null) return null;

            var attached = new List<Collider2D>();
            rb.GetAttachedColliders(attached);

            foreach (Collider2D collider in attached)
            {
                if (collider != null && !collider.isTrigger)
                    return collider;
            }

            return null;
        }

        private void EndDash()
        {
            isDashing = false;
            // Якщо керування тим часом вимкнув стан гри — не лишаємо гравцю швидкість, з якою він "їхав" би далі.
            playerRb.linearVelocity = playerMovement.enabled ? originalVelocity : Vector2.zero;
            playerMovement.IsMovementOverridden = false;
            _damagedThisDash.Clear();

            // Одразу повертаємо зіткнення з тими, кого вже пролетіли; решту — коли розійдемося.
            RestoreSeparatedColliders();
        }
    }
}
