using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.ProgressService;
using CodeBase.Player;

namespace CodeBase.UI
{
    public class HudUI : MonoBehaviour
    {
        [SerializeField] private Image _hpBarFill;
        [SerializeField] private Image _staminaBarFill;
        [SerializeField] private Slider _xpBar;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _staminaText;

        private IProgressService _progressService;
        private IPersistentProgressService _persistentProgressService;
        private PlayerStamina _playerStamina;

        public void Construct(IProgressService progressService, 
            IPersistentProgressService persistentProgressService, 
            PlayerStamina playerStamina)
        {
            _playerStamina = playerStamina;
            _progressService = progressService;
            _persistentProgressService = persistentProgressService;
            Subscribe();
            UpdateLevel();
            UpdateXpBar();
            UpdateStaminaBar();
        }
        
        private void OnDestroy() => Unsubscribe();

        private void Subscribe()
        {
            _progressService.OnLevelUp += OnLevelUp;
            _progressService.OnXpGained += OnXpGained;
            _playerStamina.OnStaminaChanged += UpdateStaminaBar;
        }

        private void Unsubscribe()
        {
            _progressService.OnLevelUp -= OnLevelUp;
            _progressService.OnXpGained -= OnXpGained;
            _playerStamina.OnStaminaChanged -= UpdateStaminaBar;
        }

        private void UpdateStaminaBar()
        {
            _staminaBarFill.fillAmount = _playerStamina.CurrentStamina / _playerStamina.MaxStamina;
            
            if (_staminaText != null)
            {
                _staminaText.text = $"{Mathf.CeilToInt(_playerStamina.CurrentStamina)}/{_playerStamina.MaxStamina}";
            }
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
        }

        private void UpdateLevel()
        {
            _levelText.text = $"lvl {_persistentProgressService.Progress.playerState.currentLevel}";
        }
    }
}
