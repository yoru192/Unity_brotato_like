using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Factory;
using CodeBase.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeBase.Infrastructure.States
{
    public class GameOverState : IState
    {
        private const string MenuScene = "Menu";
        
        private readonly IGameStateMachine _stateMachine;
        private readonly IPersistentProgressService _progressService;
        private readonly IGameFactory _gameFactory;

        public GameOverState(IGameStateMachine stateMachine, IGameFactory gameFactory)
        {
            _stateMachine = stateMachine;
            _gameFactory = gameFactory;
        }

        public void Enter()
        {
            ShowGameOverUI();
        }

        public void Exit()
        {
        }

        private async void ShowGameOverUI()
        {
            GameObject gameOverScreen = await _gameFactory.CreateGameOverScreen();
            gameOverScreen.GetComponent<GameOverUI>().Construct(this);
        }

        public void Restart()
        {
            _stateMachine.Enter<BootstrapState>();
        }

        public void ReturnToMenu()
        {
            SceneManager.LoadScene(MenuScene);
        }
        
    }
    
}