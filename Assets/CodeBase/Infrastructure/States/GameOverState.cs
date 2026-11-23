using System;
using System.Threading.Tasks;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.MetaProgress;
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
        private readonly IMetaProgressService _metaProgress;

        public GameOverState(IGameStateMachine stateMachine, IGameFactory gameFactory, IShopService shopService,
            IMetaProgressService metaProgress)
        {
            _stateMachine = stateMachine;
            _gameFactory = gameFactory;
            _shopService = shopService;
            _metaProgress = metaProgress;
        }

        public void Enter()
        {
            Time.timeScale = 0f;
            SetHudVisible(false);
            RunSummary summary = _metaProgress.CompleteRun(won: false);
            _ = ShowGameOverUI(summary);
        }

        public void Exit()
        {
            Time.timeScale = 1f;
            SetHudVisible(true);
            _shopService.StopShopTimer();
        }

        private async Task ShowGameOverUI(RunSummary summary)
        {
            try
            {
                GameObject gameOverScreen = await _gameFactory.CreateGameOverScreen();
                gameOverScreen.GetComponent<GameOverUI>().Construct(this, summary);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void SetHudVisible(bool visible)
        {
            if (_gameFactory.Hud != null)
                _gameFactory.Hud.SetVisible(visible);
        }

        public void Restart()
        {
            _stateMachine.Enter<BootstrapState>();
        }

        public void ReturnToMenu()
        {
            _stateMachine.Enter<MenuState>();
        }
    }
}
