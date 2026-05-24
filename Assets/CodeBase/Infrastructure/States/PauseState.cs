using System;
using System.Threading.Tasks;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.ShopService;
using CodeBase.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeBase.Infrastructure.States
{
    public class PauseState : IState
    {
        private const string MenuScene = "Menu";

        private readonly IGameStateMachine _stateMachine;
        private readonly IGameFactory _gameFactory;
        private readonly IShopService _shopService;
        private GameObject _pauseScreen;

        public PauseState(IGameStateMachine stateMachine, IGameFactory gameFactory, IShopService shopService)
        {
            _stateMachine = stateMachine;
            _gameFactory = gameFactory;
            _shopService = shopService;
        }

        public void Enter()
        {
            Time.timeScale = 0f;
            _ = ShowPauseMenu();
        }

        public void Exit()
        {
            Time.timeScale = 1f;
            if (_pauseScreen != null)
                UnityEngine.Object.Destroy(_pauseScreen);
        }

        private async Task ShowPauseMenu()
        {
            try
            {
                _pauseScreen = await _gameFactory.CreatePauseScreen();
                _pauseScreen.GetComponent<PauseUI>().Construct(this);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void Continue() => _stateMachine.Enter<GameLoopState>();

        public void Restart() => _stateMachine.Enter<BootstrapState>();

        public void GiveUp()
        {
            _shopService.StopShopTimer();
            SceneManager.LoadScene(MenuScene);
        }
    }
}
