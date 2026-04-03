using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.ShopService;
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
        private readonly IShopService _shopService;

        public GameOverState(IGameStateMachine stateMachine, IGameFactory gameFactory, IShopService shopService)
        {
            _stateMachine = stateMachine;
            _gameFactory = gameFactory;
            _shopService = shopService;
        }

        public void Enter()
        {
            ShowGameOverUI();
        }

        public void Exit()
        {
            _shopService.StopShopTimer();
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