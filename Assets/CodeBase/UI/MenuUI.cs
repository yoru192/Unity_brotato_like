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
            if (_stateMachine != null)
                _stateMachine.Enter<LoadLevelState, string>("Initial");
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