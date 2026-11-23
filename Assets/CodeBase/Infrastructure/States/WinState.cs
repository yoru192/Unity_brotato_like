using System;
using System.Threading.Tasks;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.MetaProgress;
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
        private readonly IMetaProgressService _metaProgress;

        public WinState(IGameStateMachine stateMachine, IGameFactory gameFactory, IShopService shopService,
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
            RunSummary summary = _metaProgress.CompleteRun(won: true);
            _ = ShowWinUI(summary);
        }

        public void Exit()
        {
            Time.timeScale = 1f;
            SetHudVisible(true);
            _shopService.StopShopTimer();
        }

        private async Task ShowWinUI(RunSummary summary)
        {
            try
            {
                GameObject winScreen = await _gameFactory.CreateWinScreen();
                winScreen.GetComponent<WinUI>().Construct(this, summary);
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
