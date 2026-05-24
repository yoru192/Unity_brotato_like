using CodeBase.Infrastructure.States;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public class PauseUI : MonoBehaviour
    {
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _giveUpButton;
        [SerializeField] private Button _settingsButton;

        private PauseState _pauseState;

        public void Construct(PauseState pauseState)
        {
            _pauseState = pauseState;
            _continueButton.onClick.AddListener(OnContinue);
            _restartButton.onClick.AddListener(OnRestart);
            _giveUpButton.onClick.AddListener(OnGiveUp);
            _settingsButton.onClick.AddListener(OnSettings);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                _pauseState?.Continue();
        }

        private void OnContinue() => _pauseState.Continue();
        private void OnRestart() => _pauseState.Restart();
        private void OnGiveUp() => _pauseState.GiveUp();
        private void OnSettings() { }

        private void OnDestroy()
        {
            _continueButton.onClick.RemoveListener(OnContinue);
            _restartButton.onClick.RemoveListener(OnRestart);
            _giveUpButton.onClick.RemoveListener(OnGiveUp);
            _settingsButton.onClick.RemoveListener(OnSettings);
        }
    }
}
