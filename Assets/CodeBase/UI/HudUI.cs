using System;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.Balance;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.ProgressService;
using CodeBase.Infrastructure.Services.Upgrade;
using CodeBase.Logic;
using CodeBase.Player;
using CodeBase.Player.Movement;
using CodeBase.StaticData;
using CodeBase.StaticData.Weapon;
using UnityEngine.Serialization;

namespace CodeBase.UI
{
    public class HudUI : MonoBehaviour, ISavedProgressReader
    {
        [SerializeField] private Image _hpBarFill;
        [SerializeField] private Image _staminaBarFill;
        [SerializeField] private Slider _xpBar;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _staminaAmount;
        [SerializeField] private TextMeshProUGUI _balanceAmount;
        [SerializeField] private TextMeshProUGUI _hpAmount;
        [SerializeField] private TextMeshProUGUI _xpAmount;
        [SerializeField] private TextMeshProUGUI _weaponDamage;
        [SerializeField] private TextMeshProUGUI _weaponCooldown;
        [SerializeField] private TextMeshProUGUI _staminaRegenRate;
        [SerializeField] private TextMeshProUGUI _moveSpeed;
        

        private IProgressService _progressService;
        private IPersistentProgressService _persistentProgressService;
        private PlayerStamina _playerStamina;
        private IBalanceService _balanceService;
        private PlayerHealth _health;
        private WeaponStaticData _weaponData;
        private State _state;
        private IUpgradeService _upgradeService;
        private PlayerMovement _playerMovement;
        private PlayerStaticData _playerData;

        public void Construct( IBalanceService balanceService, IProgressService progressService, 
            IPersistentProgressService persistentProgressService, 
            PlayerStamina playerStamina, PlayerHealth health, WeaponStaticData weaponData, IUpgradeService upgradeService, PlayerMovement playerMovement, PlayerStaticData playerData )
        {
            _playerData  = playerData;
            _balanceService = balanceService;
            _playerStamina = playerStamina;
            _progressService = progressService;
            _persistentProgressService = persistentProgressService;
            _weaponData = weaponData;
            _upgradeService = upgradeService;
            _health = health;
            _playerMovement = playerMovement;
            Subscribe();
            UpdateLevel();
            UpdateXpBar();
            UpdateStamina();
            UpdateStats();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            _balanceService.OnBalanceGained += OnBalanceGained;
            _progressService.OnLevelUp += OnLevelUp;
            _progressService.OnXpGained += OnXpGained;
            _playerStamina.OnStaminaChanged += UpdateStamina;
            _health.HealthChanged += UpdateHpBar;
            _upgradeService.OnUpgradeWeapon += UpgradeWeaponHud;
            _upgradeService.OnUpgradeHP += UpdateHpBar;
            _upgradeService.OnUpgradeMoveSpeed += UpgradeMoveSpeed;
            _upgradeService.OnUpgradeStamina += UpdateStaminaStat;
        }

        private void Unsubscribe()
        {
            _balanceService.OnBalanceGained -= OnBalanceGained;
            _progressService.OnLevelUp -= OnLevelUp;
            _progressService.OnXpGained -= OnXpGained;
            _playerStamina.OnStaminaChanged -= UpdateStamina;
            _health.HealthChanged -= UpdateHpBar;
            _upgradeService.OnUpgradeWeapon -= UpgradeWeaponHud;
            _upgradeService.OnUpgradeHP -= UpdateHpBar;
            _upgradeService.OnUpgradeMoveSpeed -= UpgradeMoveSpeed;
            _upgradeService.OnUpgradeStamina -= UpdateStaminaStat;
        }

        private void UpdateHpBar()
        {
            if (_health != null && _hpBarFill != null)
            {
                _hpBarFill.fillAmount = _health.Current / _health.Max;
                if (_hpAmount != null)
                {
                    _hpAmount.text = $"{Mathf.CeilToInt(_health.Current)}/{_health.Max}";
                }
            }
        }

        private void UpdateStats()
        {
            UpgradeWeaponHud();
            UpgradeMoveSpeed();
            UpdateStaminaStat();
        }
        private void UpdateStamina()
        {
            _staminaBarFill.fillAmount = _playerStamina.CurrentStamina / _playerStamina.MaxStamina;
            
            if (_staminaAmount != null)
            {
                _staminaAmount.text = $"{Mathf.CeilToInt(_playerStamina.CurrentStamina)}/{_playerStamina.MaxStamina}";
            }
        }

        private void UpgradeMoveSpeed()
        {
            _moveSpeed.text = $"Move speed - {(_persistentProgressService.Progress.playerState.moveSpeed == 0 ? _playerData.moveSpeed : _persistentProgressService.Progress.playerState.moveSpeed)}";
        }

        private void UpdateStaminaStat()
        {
            float regen = _persistentProgressService.Progress.playerState.regenRateStamina;
            _staminaRegenRate.text = $"Stamina regen rate - {(regen == 0 ? _playerData.regenRateStamina : regen)}";
        }

        private void UpgradeWeaponHud()
        {
            _weaponDamage.text = $"Weapon damage - {_state?.weaponDamage ?? _weaponData.damage}";
            _weaponCooldown.text = $"Weapon cooldown - {_state?.weaponCooldown ?? _weaponData.cooldown}";
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
            _levelText.text = $"lvl {_persistentProgressService.Progress.playerState.currentLevel}";
        }

        private void OnBalanceGained(int obj)
        {
            UpdateBalanceAmount();
        }

        private void UpdateBalanceAmount()
        {
            _balanceAmount.text = _persistentProgressService.Progress.playerState.currentBalance.ToString();
        }

        public void LoadProgress(PlayerProgress progress)
        {
            _state = progress.playerState;
            if (_state.weaponDamage == 0 && _weaponData.damage > 0)
                _state.weaponDamage = _weaponData.damage;
            if (_state.weaponCooldown == 0 && _weaponData.cooldown > 0)
                _state.weaponCooldown = _weaponData.cooldown;
        }
    }
}
