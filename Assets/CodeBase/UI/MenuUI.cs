using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.States;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public class MenuUI : MonoBehaviour
    {
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button quitGameButton;

        private IGameStateMachine _stateMachine;

        private void Awake()
        {
            _stateMachine = AllServices.Container.Single<IGameStateMachine>();

            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClicked);

            if (quitGameButton != null)
                quitGameButton.onClick.AddListener(OnQuitGameClicked);
        }

        private void OnDestroy()
        {
            if (startGameButton != null)
                startGameButton.onClick.RemoveListener(OnStartGameClicked);

            if (quitGameButton != null)
                quitGameButton.onClick.RemoveListener(OnQuitGameClicked);
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