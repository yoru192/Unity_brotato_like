using CodeBase.Infrastructure.Services.MetaProgress;
using CodeBase.Infrastructure.States;
using CodeBase.UI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public class WinUI : MonoBehaviour
    {
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;
        [SerializeField] private RunSummaryView _runSummary;

        private WinState _winState;

        public void Construct(WinState winState, RunSummary summary)
        {
            _winState = winState;

            _restartButton.onClick.AddListener(OnRestartClick);
            _menuButton.onClick.AddListener(OnMenuClick);

            if (_runSummary != null)
                _runSummary.Show(summary);
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
