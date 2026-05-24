using System;
using System.Threading.Tasks;
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
            Time.timeScale = 0f;
            _ = ShowGameOverUI();
        }

        public void Exit()
        {
            Time.timeScale = 1f;
            _shopService.StopShopTimer();
        }

        private async Task ShowGameOverUI()
        {
            try
            {
                GameObject gameOverScreen = await _gameFactory.CreateGameOverScreen();
                gameOverScreen.GetComponent<GameOverUI>().Construct(this);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
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