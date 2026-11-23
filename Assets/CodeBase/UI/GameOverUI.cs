using CodeBase.Infrastructure.Services.MetaProgress;
using CodeBase.Infrastructure.States;
using CodeBase.UI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;
        [SerializeField] private RunSummaryView _runSummary;

        private GameOverState _gameOverState;

        public void Construct(GameOverState gameOverState, RunSummary summary)
        {
            _gameOverState = gameOverState;

            _restartButton.onClick.AddListener(OnRestartClick);
            _menuButton.onClick.AddListener(OnMenuClick);

            if (_runSummary != null)
                _runSummary.Show(summary);
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
