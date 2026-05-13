using CodeBase.Infrastructure.States;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public class WinUI : MonoBehaviour
    {
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;

        private WinState _winState;

        public void Construct(WinState winState)
        {
            _winState = winState;

            _restartButton.onClick.AddListener(OnRestartClick);
            _menuButton.onClick.AddListener(OnMenuClick);
        }

        private void OnRestartClick()
        {
            _winState.Restart();
        }

        private void OnMenuClick()
        {
            _winState.ReturnToMenu();
        }

        private void OnDestroy()
        {
            _restartButton.onClick.RemoveListener(OnRestartClick);
            _menuButton.onClick.RemoveListener(OnMenuClick);
        }
    }
}
