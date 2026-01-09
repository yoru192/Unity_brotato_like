using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Data;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.Upgrade;
using CodeBase.StaticData;
using CodeBase.UI;
using UnityEngine;

namespace CodeBase.Infrastructure.States
{
    public class UpgradeState : IState
    {
        private readonly IGameStateMachine _stateMachine;
        private readonly IGameFactory _gameFactory;
        private readonly IUpgradeService _upgradeService;
        
        private GameObject _upgradeUI;

        public UpgradeState(
            IGameStateMachine stateMachine, 
            IGameFactory gameFactory, 
            IUpgradeService upgradeService)
        {
            _stateMachine = stateMachine;
            _gameFactory = gameFactory;
            _upgradeService = upgradeService;
        }

        public void Enter()
        {
            Time.timeScale = 0f;
            ShowUpgradeOptions();
        }

        public void Exit()
        {
            Time.timeScale = 1f;
            
            if (_upgradeUI != null)
                Object.Destroy(_upgradeUI);
        }

        private async void ShowUpgradeOptions()
        {
            List<UpgradeStaticData> options = _upgradeService.GenerateUpgradeOptions(3);
            await CreateUpgradeUI(options);
        }

        private async Task CreateUpgradeUI(List<UpgradeStaticData> options)
        {
            _upgradeUI = await _gameFactory.CreateUpgradeScreen();
    
            UpgradeScreen upgradeScreen = _upgradeUI.GetComponentInChildren<UpgradeScreen>();
            if (upgradeScreen != null)
            {
                upgradeScreen.Construct(options, OnUpgradeSelected);
            }
            else
            {
                Debug.LogError("UpgradeScreen component not found!");
            }
        }


        private void OnUpgradeSelected(UpgradeStaticData selectedUpgrade)
        {
            _upgradeService.ApplyUpgrade(selectedUpgrade);
            _stateMachine.Enter<GameLoopState>();
        }

    }
}