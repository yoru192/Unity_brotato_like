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
        private IGameStateMachine _stateMachine;
        
        private void Awake()
        {
            _stateMachine = AllServices.Container.Single<IGameStateMachine>();
            if (startGameButton != null)
            {
                startGameButton.onClick.AddListener(OnStartGameClicked);
            }
            else
            {
                Debug.LogError("Start Game Button is not assigned in MenuUI!");
            }
        }

        private void OnDestroy()
        {
            if (startGameButton != null)
            {
                startGameButton.onClick.RemoveListener(OnStartGameClicked);
            }
        }

        private void OnStartGameClicked()
        {
            if (_stateMachine != null)
            {
                _stateMachine.Enter<BootstrapState>();
            }
            else
            {
                SceneManager.LoadScene("Initial");
            }
        }
    }
}