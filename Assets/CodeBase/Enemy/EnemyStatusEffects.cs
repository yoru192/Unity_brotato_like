using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Perk;
using CodeBase.Infrastructure.Services.Vfx;
using CodeBase.Logic;
using CodeBase.StaticData.Perk;
using CodeBase.StaticData.Vfx;
using Pathfinding;
using UnityEngine;

namespace CodeBase.Enemy
{
    /// <summary>
    /// Статус-ефекти на ворогові від перків гравця: горіння (Ignite) та сповільнення (Frost).
    /// Додається фабрикою автоматично; щоб налаштувати значення — додай компонент на префаб ворога.
    /// </summary>
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyStatusEffects : MonoBehaviour, IPoolable
    {
        [Header("Горіння (перк Ignite: крит підпалює; значення перку = урон за секунду)")]
        [SerializeField] private float burnDuration = 3f;
        [SerializeField] private float burnTickInterval = 0.5f;
        [SerializeField] private Color burnTint = new Color(1f, 0.6f, 0.25f);

        [Header("Сповільнення (перк Frost: значення перку = шанс у відсотках за удар)")]
        [Range(0.05f, 1f)]
        [SerializeField] private float slowSpeedFactor = 0.5f;
        [SerializeField] private float slowDuration = 2f;
        [SerializeField] private Color slowTint = new Color(0.55f, 0.8f, 1f);

        private EnemyHealth _health;
        private EnemyHitFeedback _feedback;
        private IAstarAI _ai;
        private IPerkService _perkService;
        private IVfxService _vfx;
        private GameObject _burnVfx;

        private float _burnDamagePerSecond;
        private float _burnTimeLeft;
        private float _burnTickTimer;

        private float _slowTimeLeft;
        private float _speedBeforeSlow;
        private bool _isSlowed;

        private void Awake()
        {
            _health = GetComponent<EnemyHealth>();
            _ai = GetComponent<IAstarAI>();
            _perkService = AllServices.Container.Single<IPerkService>();
            _vfx = AllServices.Container.Single<IVfxService>();
        }

        private void OnEnable()
        {
            _health.Damaged += OnDamaged;
        }

        private void OnDisable()
        {
            _health.Damaged -= OnDamaged;
            ClearEffects();
        }

        public void OnSpawn() => ClearEffects();

        public void OnDespawn() => ClearEffects();

        private void Update()
        {
            UpdateBurn();
            UpdateSlow();
        }

        // Викликається лише для ударів гравця; тіки горіння йдуть через TakeStatusDamage і сюди не потрапляють.
        private void OnDamaged(float damage, bool isCritical)
        {
            if (_perkService == null || _health.Current <= 0f) return;

            if (isCritical)
            {
                float burnDps = _perkService.GetValue(PerkId.Ignite);
                if (burnDps > 0f)
                    StartBurn(burnDps);
            }

            float slowChance = _perkService.GetValue(PerkId.Frost);
            if (slowChance > 0f && Random.Range(0f, 100f) < slowChance)
                StartSlow();
        }

        private void StartBurn(float damagePerSecond)
        {
            if (_burnTimeLeft <= 0f)
                _burnTickTimer = burnTickInterval;

            _burnDamagePerSecond = damagePerSecond;
            _burnTimeLeft = burnDuration;
            RefreshTint();

            if (_burnVfx == null && _vfx != null)
                _burnVfx = _vfx.PlayFollowing(VfxId.Burn, transform);
        }

        private void StopBurnVfx()
        {
            if (_burnVfx == null) return;

            if (_vfx != null)
                _vfx.Stop(_burnVfx);
            _burnVfx = null;
        }

        private void UpdateBurn()
        {
            if (_burnTimeLeft <= 0f) return;

            _burnTimeLeft -= Time.deltaTime;
            _burnTickTimer -= Time.deltaTime;

            if (_burnTickTimer <= 0f)
            {
                _burnTickTimer += burnTickInterval;
                _health.TakeStatusDamage(_burnDamagePerSecond * burnTickInterval);
            }

            if (_burnTimeLeft <= 0f || _health.Current <= 0f)
            {
                _burnTimeLeft = 0f;
                StopBurnVfx();
                RefreshTint();
            }
        }

        private void StartSlow()
        {
            if (!_isSlowed && _ai != null)
            {
                _speedBeforeSlow = _ai.maxSpeed;
                _ai.maxSpeed = _speedBeforeSlow * slowSpeedFactor;
                _isSlowed = true;
            }

            // Спалах льоду лише коли сповільнення накладається заново, а не при кожному продовженні.
            if (_slowTimeLeft <= 0f && _vfx != null)
                _vfx.Play(VfxId.FrostHit, transform.position);

            _slowTimeLeft = slowDuration;
            RefreshTint();
        }

        private void UpdateSlow()
        {
            if (_slowTimeLeft <= 0f) return;

            _slowTimeLeft -= Time.deltaTime;
            if (_slowTimeLeft <= 0f)
                EndSlow();
        }

        private void EndSlow()
        {
            _slowTimeLeft = 0f;

            if (_isSlowed && _ai != null)
                _ai.maxSpeed = _speedBeforeSlow;
            _isSlowed = false;

            RefreshTint();
        }

        private void ClearEffects()
        {
            _burnTimeLeft = 0f;
            StopBurnVfx();
            EndSlow();
        }

        private void RefreshTint()
        {
            if (_feedback == null && !TryGetComponent(out _feedback)) return;

            // Горіння помітніше за сповільнення, тому має пріоритет.
            Color tint = _burnTimeLeft > 0f ? burnTint : _slowTimeLeft > 0f ? slowTint : Color.white;
            _feedback.SetStatusTint(tint);
        }
    }
}
