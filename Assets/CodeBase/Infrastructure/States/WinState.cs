using System;
using System.Threading.Tasks;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.ShopService;
using CodeBase.UI;
using UnityEngine;

namespace CodeBase.Infrastructure.States
{
    public class WinState : IState
    {
        private const string MenuScene = "Menu";

        private readonly IGameStateMachine _stateMachine;
        private readonly IGameFactory _gameFactory;
        private readonly IShopService _shopService;

        public WinState(IGameStateMachine stateMachine, IGameFactory gameFactory, IShopService shopService)
        {
            _stateMachine = stateMachine;
            _gameFactory = gameFactory;
            _shopService = shopService;
        }

        public void Enter()
        {
            Time.timeScale = 0f;
            _ = ShowWinUI();
        }

        public void Exit()
        {
            Time.timeScale = 1f;
            _shopService.StopShopTimer();
        }

        private async Task ShowWinUI()
        {
            try
            {
                GameObject winScreen = await _gameFactory.CreateWinScreen();
                winScreen.GetComponent<WinUI>().Construct(this);
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
            UnityEngine.SceneManagement.SceneManager.LoadScene(MenuScene);
        }
    }
}
