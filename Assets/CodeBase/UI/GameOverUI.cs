using CodeBase.Infrastructure.States;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;

        [Header("Meta (опційно)")]
        [Tooltip("Текст із нарахованою есенцією за забіг. Можна не призначати.")]
        [SerializeField] private TextMeshProUGUI _essenceRewardText;

        private GameOverState _gameOverState;

        public void Construct(GameOverState gameOverState, int essenceReward = 0, int essenceTotal = 0)
        {
            _gameOverState = gameOverState;

            _restartButton.onClick.AddListener(OnRestartClick);
            _menuButton.onClick.AddListener(OnMenuClick);

            if (_essenceRewardText != null)
                _essenceRewardText.text = $"+{essenceReward} essence (total: {essenceTotal})";
        }

        private void OnRestartClick()
        {
            _gameOverState.Restart();
        }

        private void OnMenuClick()
        {
            _gameOverState.ReturnToMenu();
        }

        private void OnDestroy()
        {
            _restartButton.onClick.RemoveListener(OnRestartClick);
            _menuButton.onClick.RemoveListener(OnMenuClick);
        }
    }
}
