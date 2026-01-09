using UnityEngine;
using UnityEngine.UI;
using CodeBase.Infrastructure.States;

namespace CodeBase.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;
        
        private GameOverState _gameOverState;

        public void Construct(GameOverState gameOverState)
        {
            _gameOverState = gameOverState;
            
            _restartButton.onClick.AddListener(OnRestartClick);
            _menuButton.onClick.AddListener(OnMenuClick);
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