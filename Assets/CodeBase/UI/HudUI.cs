using CodeBase.Ability;
using CodeBase.Data;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Balance;
using CodeBase.Infrastructure.Services.Buff;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.ProgressService;
using CodeBase.Infrastructure.Services.Settings;
using CodeBase.Infrastructure.Services.Upgrade;
using CodeBase.Logic;
using CodeBase.Player;
using CodeBase.StaticData;
using CodeBase.StaticData.Ability;
using CodeBase.StaticData.Hero;
using CodeBase.StaticData.Weapon;

namespace CodeBase.UI
{
    public class HudUI : MonoBehaviour, ISavedProgressReader
    {
        [Header("HP")]
        [SerializeField] private Image _hpBarFill;
        [SerializeField] private TextMeshProUGUI _hpAmount;
        [Header("XP")]
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Slider _xpBar;
        [SerializeField] private TextMeshProUGUI _xpAmount;
        [Header("Stamina")]
        [SerializeField] private Image _staminaBarFill;
        [SerializeField] private TextMeshProUGUI _staminaAmount;
        [Header("Balance")]
        [SerializeField] private TextMeshProUGUI _balanceAmount;
        [Header("Stats")]
        [SerializeField] private GameObject _statsPanel;
        [SerializeField] private TextMeshProUGUI _meleeDamage;
        [SerializeField] private TextMeshProUGUI _meleeCooldown;
        [SerializeField] private TextMeshProUGUI _rangedDamage;
        [SerializeField] private TextMeshProUGUI _rangedShootRate;
        [SerializeField] private TextMeshProUGUI _staminaRegenRate;
        [SerializeField] private TextMeshProUGUI _moveSpeed;
        [Header("Wave")]
        [SerializeField] private TextMeshProUGUI _waveText;
        [Tooltip("Плашка з лічильником хвиль; ховається в бою з босом, на її місці — смуга HP боса.")]
        [SerializeField] private GameObject _wavePanel;
        [Header("Boss")]
        [SerializeField] private CodeBase.UI.Elements.BossHealthBar _bossHealthBar;
        [Header("Ability")]
        [SerializeField] private Image _abilityIcon;
        [SerializeField] private Slider _abilitySlider;
        [SerializeField] private TextMeshProUGUI _abilityTextTime;
        [SerializeField] private Color _abilityReadyColor = Color.white;
        [SerializeField] private Color _abilityCooldownColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        private IProgressService _progressService;
        private IPersistentProgressService _persistentProgressService;
        private PlayerStamina _playerStamina;
        private IBalanceService _balanceService;
        private PlayerHealth _health;
        private WeaponStaticData _meleeWeaponData;
        private WeaponStaticData _rangedWeaponData;
        private IUpgradeService _upgradeService;
        private IStaticDataService _staticData;
        private IBuffService _buffService;
        private HeroStaticData _heroData;
        private WaveController _waveController;
        private AbilityController _abilityController;
        private IAbility _currentAbility;
        private ISettingsService _settings;
        private Canvas _canvas;

        public void Construct( IBalanceService balanceService, IProgressService progressService, 
            IPersistentProgressService persistentProgressService, 
            PlayerStamina playerStamina,
            PlayerHealth health,
            WeaponStaticData meleeWeaponData,
            WeaponStaticData rangedWeaponData,
            IUpgradeService upgradeService,
            IStaticDataService staticData,
            IBuffService buffService,
            AbilityController abilityController)
        {
            _staticData  = staticData;
            _balanceService = balanceService;
            _playerStamina = playerStamina;
            _progressService = progressService;
            _persistentProgressService = persistentProgressService;
            _meleeWeaponData = meleeWeaponData;
            _rangedWeaponData = rangedWeaponData;
            _upgradeService = upgradeService;
            _health = health;
            _buffService = buffService;
            _abilityController = abilityController;
            
            _heroData = _staticData.ForHero(_persistentProgressService.Progress.worldData.selectedHero);
            
             _settings = AllServices.Container.Single<ISettingsService>();
            
            Subscribe();
            UpdateLevel();
            UpdateXpBar();
            UpdateStamina();
            UpdateStats();
        }

        /// <summary>
        /// Hides/shows the HUD under full-screen overlays (pause, win, game over). Only the Canvas is
        /// toggled, so components on this object (e.g. PauseInputHandler) keep running.
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (_canvas == null)
                _canvas = GetComponent<Canvas>();
            if (_canvas != null)
                _canvas.enabled = visible;
        }

        private void Update()
        {
            UpdateAbilityUI();
        }

        private void UpdateAbilityUI()
        {
            if (_abilityController == null || _abilityController.AbilityCount == 0)
                return;

            IAbility ability = _abilityController.GetAbility(0);
            if (ability == null)
                return;

            if (ability != _currentAbility)
            {
                _currentAbility = ability;
                if (_abilityIcon != null)
                    _abilityIcon.sprite = ability.Icon;
            }

            bool isReady = ability.IsReady;
            float cooldownProgress = ability.Cooldown > 0f
                ? Mathf.Clamp01(ability.RemainingCooldown / ability.Cooldown)
                : 0f;

            if (_abilitySlider != null)
                _abilitySlider.value = cooldownProgress;

            if (_abilityTextTime != null)
                _abilityTextTime.text = isReady ? string.Empty : Mathf.CeilToInt(ability.RemainingCooldown).ToString();

            if (_abilityIcon != null)
                _abilityIcon.color = isReady ? _abilityReadyColor : _abilityCooldownColor;
        }
        public void SetWaveController(WaveController waveController)
        {
            _waveController = waveController;
            _waveController.OnWaveCompleted += UpdateWaveText;
            _waveController.OnBossSpawned += OnBossSpawned;
            UpdateWaveText();
        }

        private void OnDestroy()
        {
            Unsubscribe();
            if (_waveController != null)
            {
                _waveController.OnWaveCompleted -= UpdateWaveText;
                _waveController.OnBossSpawned -= OnBossSpawned;
            }
        }

        private void OnBossSpawned(GameObject boss)
        {
            if (_bossHealthBar != null)
                _bossHealthBar.Show(boss);
        }

        private void Subscribe()
        {
            _balanceService.OnBalanceChanged += OnBalanceGained;
            _progressService.OnLevelUp += OnLevelUp;
            _progressService.OnXpGained += OnXpGained;
            _playerStamina.OnStaminaChanged += UpdateStamina;
            _health.HealthChanged += UpdateHpBar;
            _upgradeService.OnUpgradeWeapon += UpdateWeaponHud;
            _upgradeService.OnUpgradeHP += UpdateHpBar;
            _upgradeService.OnUpgradeMoveSpeed += UpdateMoveSpeed;
            _upgradeService.OnUpgradeStamina += UpdateStaminaStat;
            _buffService.OnWeaponBuffChanged += UpdateWeaponHud;
            _buffService.OnHealthBuffChanged += UpdateHpBar;
            _buffService.OnSpeedBuffChanged += UpdateMoveSpeed;
            _settings.Changed += UpdateStats;

        }

        private void Unsubscribe()
        {
            _balanceService.OnBalanceChanged -= OnBalanceGained;
            _progressService.OnLevelUp -= OnLevelUp;
            _progressService.OnXpGained -= OnXpGained;
            _playerStamina.OnStaminaChanged -= UpdateStamina;
            _health.HealthChanged -= UpdateHpBar;
            _upgradeService.OnUpgradeWeapon -= UpdateWeaponHud;
            _upgradeService.OnUpgradeHP -= UpdateHpBar;
            _upgradeService.OnUpgradeMoveSpeed -= UpdateMoveSpeed;
            _upgradeService.OnUpgradeStamina -= UpdateStaminaStat;
            _buffService.OnWeaponBuffChanged -= UpdateWeaponHud;
            _buffService.OnHealthBuffChanged -= UpdateHpBar;
            _buffService.OnSpeedBuffChanged  -= UpdateMoveSpeed;
            _settings.Changed -= UpdateStats;
        }

        private void UpdateHpBar()
        {
            if (_health != null && _hpBarFill != null)
            {
                SetFill(_hpBarFill, _health.Current / _health.Max);
                if (_hpAmount != null)
                {
                    _hpAmount.text = $"{Mathf.CeilToInt(_health.Current)}/{_health.Max}";
                }
            }
        }

        private void UpdateStats()
        {
            bool show = _settings.Stats;
            _statsPanel.SetActive(show);

            if (!show) return;

            UpdateWeaponHud();
            UpdateMoveSpeed();
            UpdateStaminaStat();
        }
        private void UpdateStamina()
        {
            SetFill(_staminaBarFill, _playerStamina.CurrentStamina / _playerStamina.MaxStamina);
            
            if (_staminaAmount != null)
            {
                _staminaAmount.text = $"{Mathf.CeilToInt(_playerStamina.CurrentStamina)}/{_playerStamina.MaxStamina}";
            }
        }

        private void UpdateMoveSpeed() =>
            _moveSpeed.text = $"Move speed - {Stat(_persistentProgressService.Progress.playerState.moveSpeed, _heroData.moveSpeed)}";

        private void UpdateStaminaStat() =>
            _staminaRegenRate.text = $"Stamina regen rate - {Stat(_persistentProgressService.Progress.playerState.regenRateStamina, _heroData.regenRateStamina)}";

        private void UpdateWeaponHud()
        {
            var state = _persistentProgressService.Progress.playerState;
            _meleeDamage.text = $"Melee damage - {Stat(state.MeleeWeaponState.weaponDamage, _meleeWeaponData.damage)}";
            _meleeCooldown.text = $"Melee cooldown - {Stat(state.MeleeWeaponState.weaponCooldown, _meleeWeaponData.cooldown)}";
            _rangedDamage.text = $"Ranged damage - {Stat(state.ArrowWeaponState.weaponDamage, _rangedWeaponData.damage)}";
            _rangedShootRate.text = $"Ranged shoot rate - {Stat(state.ArrowWeaponState.weaponCooldown, _rangedWeaponData.cooldown)}";
        }

        private void UpdateWaveText()
        {
            if (_waveText == null || _waveController == null) return;

            // У бою з босом хвилі лише фон — лічильник нічого не означає, замість нього смуга HP боса.
            if (_wavePanel != null)
                _wavePanel.SetActive(!_waveController.IsBossFight);
            if (_waveController.IsBossFight)
                return;

            int displayed = Mathf.Max(1, _waveController.currentWave);
            _waveText.text = $"Wave {displayed}/{_waveController.MaxWaves}";
        }

        private static float Stat(float value, float defaultValue) => value == 0 ? defaultValue : value;

        // Bar fills are 9-sliced (rounded ends), which Image.fillAmount ignores — resize via anchors instead.
        private static void SetFill(Image fill, float ratio)
        {
            // NaN-safe: max can still be 0 when the HUD is constructed, and a NaN anchor sticks in the RectTransform.
            ratio = ratio > 0f ? Mathf.Min(ratio, 1f) : 0f;
            RectTransform rect = fill.rectTransform;
            rect.anchorMax = new Vector2(ratio, rect.anchorMax.y);
            fill.enabled = ratio > 0f;
        }

        private void OnXpGained(int amount)
        {
            UpdateXpBar();
        }

        private void OnLevelUp(int level)
        {
            UpdateLevel();
            UpdateXpBar();
        }

        private void UpdateXpBar()
        {
            int currentXp = _persistentProgressService.Progress.playerState.currentXp;
            int requiredXp = _progressService.CalculateXpForNextLevel();
            _xpBar.value = (float)currentXp / requiredXp;
            if (_xpAmount != null)
            {
                _xpAmount.text = $"{Mathf.CeilToInt(currentXp)}/{requiredXp}";
            }
        }

        private void UpdateLevel()
        {
            _levelText.text = $"Lv {_persistentProgressService.Progress.playerState.currentLevel}";
        }

        private void OnBalanceGained()
        {
            UpdateBalanceAmount();
        }

        private void UpdateBalanceAmount()
        {
            _balanceAmount.text = _persistentProgressService.Progress.playerState.currentBalance.ToString();
        }

        public void LoadProgress(PlayerProgress progress)
        {
            var melee = progress.playerState.MeleeWeaponState;
            if (melee.weaponDamage == 0 && _meleeWeaponData.damage > 0)
                melee.weaponDamage = _meleeWeaponData.damage;
            if (melee.weaponCooldown == 0 && _meleeWeaponData.cooldown > 0)
                melee.weaponCooldown = _meleeWeaponData.cooldown;
        }

    }
}
