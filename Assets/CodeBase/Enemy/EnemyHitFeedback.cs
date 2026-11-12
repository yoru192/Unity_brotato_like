using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.HitFeel;
using CodeBase.Logic;
using Pathfinding;
using UnityEngine;

namespace CodeBase.Enemy
{
    /// <summary>
    /// Реакція ворога на удар: спалах спрайтів, відкид від гравця, легка тряска камери на криті.
    /// Додається фабрикою автоматично; щоб налаштувати значення — додай компонент на префаб ворога.
    /// </summary>
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyHitFeedback : MonoBehaviour, IPoolable
    {
        [Header("Спалах")]
        [SerializeField] private Color flashColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private float flashDuration = 0.1f;

        [Header("Віддача (лише візуал, агент не рухається)")]
        [SerializeField] private float recoilDistance = 0.4f;
        [SerializeField] private float recoilDuration = 0.2f;
        [Tooltip("Частка тривалості, за яку спрайт відлітає на повну дистанцію; решту часу повертається.")]
        [Range(0.05f, 0.95f)]
        [SerializeField] private float recoilOutPortion = 0.25f;

        [Header("Крит")]
        [SerializeField] private float critRecoilMultiplier = 1.8f;
        [SerializeField] private float critShakeAmplitude = 0.1f;
        [SerializeField] private float critShakeDuration = 0.1f;

        private EnemyHealth _health;
        private AIDestinationSetter _destinationSetter;
        private IHitFeelService _hitFeel;

        private SpriteRenderer[] _renderers;
        private Color[] _originalColors;
        private bool _isFlashing;
        private float _flashTimer;

        private Transform _visual;
        private Vector3 _visualBaseLocalPosition;
        private Vector3 _recoilLocalOffset;
        private float _recoilTimer;

        private void Awake()
        {
            _health = GetComponent<EnemyHealth>();
            _destinationSetter = GetComponent<AIDestinationSetter>();
            _hitFeel = AllServices.Container.Single<IHitFeelService>();

            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
            _originalColors = new Color[_renderers.Length];

            // Зсуваємо лише дочірній об'єкт зі спрайтом, щоб не втручатись у pathfinding.
            EnemyGFX gfx = GetComponentInChildren<EnemyGFX>(true);
            if (gfx != null && gfx.transform != transform)
            {
                _visual = gfx.transform;
                _visualBaseLocalPosition = _visual.localPosition;
            }
        }

        private void OnEnable()
        {
            _health.Damaged += OnDamaged;
        }

        private void OnDisable()
        {
            _health.Damaged -= OnDamaged;
            ResetState();
        }

        public void OnSpawn() => ResetState();

        public void OnDespawn() => ResetState();

        private void Update()
        {
            UpdateFlash();
            UpdateRecoil();
        }

        private void OnDamaged(float damage, bool isCritical)
        {
            StartFlash();
            StartRecoil(isCritical ? recoilDistance * critRecoilMultiplier : recoilDistance);

            if (isCritical && _hitFeel != null)
                _hitFeel.Shake(critShakeAmplitude, critShakeDuration);
        }

        private void StartFlash()
        {
            if (flashDuration <= 0f) return;

            if (!_isFlashing)
            {
                for (int i = 0; i < _renderers.Length; i++)
                {
                    if (_renderers[i] == null) continue;
                    _originalColors[i] = _renderers[i].color;
                }
            }

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null) continue;
                Color color = flashColor;
                color.a = _originalColors[i].a;
                _renderers[i].color = color;
            }

            _isFlashing = true;
            _flashTimer = flashDuration;
        }

        private void UpdateFlash()
        {
            if (!_isFlashing) return;

            _flashTimer -= Time.deltaTime;
            if (_flashTimer <= 0f)
                RestoreColors();
        }

        private void RestoreColors()
        {
            if (!_isFlashing) return;

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null) continue;
                _renderers[i].color = _originalColors[i];
            }

            _isFlashing = false;
        }

        private void StartRecoil(float distance)
        {
            if (_visual == null || distance <= 0f || recoilDuration <= 0f) return;
            if (_destinationSetter == null || _destinationSetter.target == null) return;

            Vector2 away = (Vector2)transform.position - (Vector2)_destinationSetter.target.position;
            if (away.sqrMagnitude < 0.0001f) return;

            Vector3 worldOffset = away.normalized * distance;
            Transform parent = _visual.parent;
            _recoilLocalOffset = parent != null ? parent.InverseTransformVector(worldOffset) : worldOffset;
            _recoilTimer = recoilDuration;
        }

        private void UpdateRecoil()
        {
            if (_recoilTimer <= 0f) return;

            _recoilTimer -= Time.deltaTime;
            if (_recoilTimer <= 0f)
            {
                _visual.localPosition = _visualBaseLocalPosition;
                return;
            }

            // Сила 0 -> 1 за recoilOutPortion, далі плавно назад до 0.
            float progress = 1f - _recoilTimer / recoilDuration;
            float strength = progress < recoilOutPortion
                ? progress / recoilOutPortion
                : 1f - (progress - recoilOutPortion) / (1f - recoilOutPortion);

            _visual.localPosition = _visualBaseLocalPosition + _recoilLocalOffset * strength;
        }

        private void ResetState()
        {
            RestoreColors();

            if (_recoilTimer > 0f && _visual != null)
                _visual.localPosition = _visualBaseLocalPosition;
            _recoilTimer = 0f;
        }
    }
}
