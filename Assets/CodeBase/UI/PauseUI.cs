using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Settings;
using CodeBase.Infrastructure.States;
using CodeBase.UI.Screens;
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
        [SerializeField] private SettingsScreen _settingsScreen;

        private PauseState _pauseState;
        private bool _settingsOpen;
        private int _settingsClosedFrame = -1;

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
            // Поки відкриті налаштування — Esc обробляє SettingsScreen.
            // Перевірка кадру потрібна, бо обидва Update можуть спрацювати в одному кадрі
            // і той самий Esc інакше закрив би ще й паузу.
            if (_settingsOpen || Time.frameCount == _settingsClosedFrame)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
                _pauseState?.Continue();
        }

        private void OnContinue() => _pauseState.Continue();
        private void OnRestart() => _pauseState.Restart();
        private void OnGiveUp() => _pauseState.GiveUp();

        private void OnSettings()
        {
            ISettingsService settings = AllServices.Container.Single<ISettingsService>();
            if (_settingsScreen == null || settings == null)
            {
                Debug.LogError("SettingsScreen не призначений у PauseUI або ISettingsService не зареєстрований.");
                return;
            }

            _settingsOpen = true;
            _settingsScreen.Open(settings, OnSettingsClosed);
        }

        private void OnSettingsClosed()
        {
            _settingsOpen = false;
            _settingsClosedFrame = Time.frameCount;
        }

        private void OnDestroy()
        {
            _continueButton.onClick.RemoveListener(OnContinue);
            _restartButton.onClick.RemoveListener(OnRestart);
            _giveUpButton.onClick.RemoveListener(OnGiveUp);
            _settingsButton.onClick.RemoveListener(OnSettings);
        }
    }
}