using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Settings;
using CodeBase.Infrastructure.States;
using CodeBase.UI.Screens;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public class MenuUI : MonoBehaviour
    {
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button quitGameButton;

        [Header("Settings (опційно)")]
        [SerializeField] private Button settingsButton;
        [Tooltip("Неактивний екземпляр префаба SettingsScreen у цій сцені.")]
        [SerializeField] private SettingsScreen settingsScreen;

        private IGameStateMachine _stateMachine;
        private ISettingsService _settings;

        private void Awake()
        {
            _stateMachine = AllServices.Container.Single<IGameStateMachine>();
            _settings = AllServices.Container.Single<ISettingsService>();

            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClicked);

            if (quitGameButton != null)
                quitGameButton.onClick.AddListener(OnQuitGameClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        private void OnDestroy()
        {
            if (startGameButton != null)
                startGameButton.onClick.RemoveListener(OnStartGameClicked);

            if (quitGameButton != null)
                quitGameButton.onClick.RemoveListener(OnQuitGameClicked);

            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OnSettingsClicked);
        }

        private void OnSettingsClicked()
        {
            if (settingsScreen != null)
                settingsScreen.Open(_settings, null);
        }

        private void OnStartGameClicked()
        {
            // "Грати" починає новий забіг із самого початку (Initial -> новий прогрес -> вибір героя).
            // LoadLevelState тут не підходить: він завантажує бойовий рівень, а "Initial" рівнем не є.
            if (_stateMachine != null)
                _stateMachine.Enter<BootstrapState>();
            else
                SceneManager.LoadScene("Initial");
        }

        private void OnQuitGameClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}