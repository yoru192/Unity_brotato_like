using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public class MenuUI : MonoBehaviour
    {
        [SerializeField] private Button startGameButton;

        private void Awake()
        {
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
            SceneManager.LoadScene("Initial");
        }
    }
}