using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.HitFeel;
using UnityEngine;

namespace CodeBase.Player
{
    /// <summary>
    /// Реакція на урон по гравцю: тряска камери і короткий hit stop.
    /// Додається фабрикою автоматично; щоб налаштувати значення — додай компонент на префаб гравця.
    /// </summary>
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerHitFeedback : MonoBehaviour
    {
        [SerializeField] private float shakeAmplitude = 0.35f;
        [SerializeField] private float shakeDuration = 0.2f;
        [SerializeField] private float hitStopDuration = 0.06f;

        private PlayerHealth _health;
        private IHitFeelService _hitFeel;
        private float _lastHealth;
        private bool _hasBaseline;

        private void Awake()
        {
            _health = GetComponent<PlayerHealth>();
            _hitFeel = AllServices.Container.Single<IHitFeelService>();
        }

        private void OnEnable()
        {
            _hasBaseline = false;
            _health.HealthChanged += OnHealthChanged;
        }

        private void OnDisable()
        {
            _health.HealthChanged -= OnHealthChanged;
        }

        private void OnHealthChanged()
        {
            float current = _health.Current;
            // Перше значення (завантаження прогресу) лише запам'ятовуємо — це ще не урон.
            bool tookDamage = _hasBaseline && current < _lastHealth;
            _lastHealth = current;
            _hasBaseline = true;

            if (!tookDamage || _hitFeel == null) return;

            _hitFeel.Shake(shakeAmplitude, shakeDuration);
            _hitFeel.HitStop(hitStopDuration);
        }
    }
}
