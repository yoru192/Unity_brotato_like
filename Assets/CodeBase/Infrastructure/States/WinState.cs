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
            int essenceReward = _metaProgress.CompleteRun(won: true);
            _ = ShowWinUI(essenceReward);
        }

        public void Exit()
        {
            Time.timeScale = 1f;
            _shopService.StopShopTimer();
        }

        private async Task ShowWinUI(int essenceReward)
        {
            try
            {
                GameObject winScreen = await _gameFactory.CreateWinScreen();
                winScreen.GetComponent<WinUI>().Construct(this, essenceReward, _metaProgress.Essence);
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
            _stateMachine.Enter<MenuState>();
        }
    }
}
