using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;

namespace CodeBase.UI
{
    public class HudUI : MonoBehaviour
    {
        [SerializeField] private Image _hpBarFill;
        [SerializeField] private Slider _xpBar;
        [SerializeField] private TextMeshProUGUI _levelText;
        
        private IProgressService _progressService;
        private IPersistentProgressService _persistentProgressService;

        public void Construct(IProgressService progressService, IPersistentProgressService persistentProgressService)
        {
            _progressService = progressService;
            _persistentProgressService = persistentProgressService;
            
            Subscribe();
            UpdateLevel();
            UpdateXpBar();
        }

        private void OnDestroy() => Unsubscribe();

        private void Subscribe()
        {
            _progressService.OnLevelUp += OnLevelUp;
            _progressService.OnXpGained += OnXpGained;
        }

        private void Unsubscribe()
        {
            _progressService.OnLevelUp -= OnLevelUp;
            _progressService.OnXpGained -= OnXpGained;
        }

        public void UpdateHealthBar(float current, float max)
        {
            _hpBarFill.fillAmount = current / max;
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