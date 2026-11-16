using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.Balance;
using CodeBase.Infrastructure.Services.MetaProgress;
using CodeBase.Infrastructure.Services.Upgrade;
using CodeBase.StaticData;
using CodeBase.StaticData.MetaUpgrade;
using CodeBase.UI;
using CodeBase.UI.Screens;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeBase.Infrastructure.States
{
    public class UpgradeState : IState
    {
        private const int OptionsCount = 4;
        private const int BaseRerollCost = 5;
        private const int RerollCostStep = 5;

        private readonly IGameStateMachine _stateMachine;
        private readonly IGameFactory _gameFactory;
        private readonly IUpgradeService _upgradeService;
        private readonly IBalanceService _balanceService;
        private readonly IMetaProgressService _metaProgress;

        private GameObject _upgradeUI;
        private UpgradeScreen _upgradeScreen;
        private int _rerollCost;
        private int _freeRerolls;

        public UpgradeState(
            IGameStateMachine stateMachine,
            IGameFactory gameFactory,
            IUpgradeService upgradeService,
            IBalanceService balanceService,
            IMetaProgressService metaProgress)
        {
            _metaProgress = metaProgress;
            _stateMachine = stateMachine;
            _gameFactory = gameFactory;
            _upgradeService = upgradeService;
            _balanceService = balanceService;
        }

        public void Enter()
        {
            _rerollCost = BaseRerollCost;
            _freeRerolls = Mathf.RoundToInt(_metaProgress.GetUpgradeValue(MetaUpgradeId.FreeReroll));
            _ = ShowUpgradeOptions();
        }

        public void Exit()
        {
            Time.timeScale = 1f;

            _upgradeScreen = null;
            if (_upgradeUI != null)
                Object.Destroy(_upgradeUI);
        }

        private async Task ShowUpgradeOptions()
        {
            try
            {
                List<UpgradeStaticData> options = _upgradeService.GenerateUpgradeOptions(OptionsCount);
                await CreateUpgradeUI(options);
                Time.timeScale = 0f;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async Task CreateUpgradeUI(List<UpgradeStaticData> options)
        {
            _upgradeUI = await _gameFactory.CreateUpgradeScreen();

            _upgradeScreen = _upgradeUI.GetComponentInChildren<UpgradeScreen>();
            if (_upgradeScreen != null)
            {
                _upgradeScreen.Construct(options, OnUpgradeSelected, OnRerollRequested);
                UpdateRerollUI();
            }
            else
            {
                Debug.LogError("UpgradeScreen component not found!");
            }
        }

        private void OnRerollRequested()
        {
            // Free rerolls (Free Reroll meta upgrade) go first and do not escalate the price.
            // Paid reroll: charge first, then escalate the next price and re-roll the options.
            if (_freeRerolls > 0)
            {
                _freeRerolls--;
            }
            else
            {
                if (!_balanceService.TryRemoveBalance(_rerollCost))
                    return;

                _rerollCost += RerollCostStep;
            }

            List<UpgradeStaticData> options = _upgradeService.GenerateUpgradeOptions(OptionsCount);
            _upgradeScreen.ShowOptions(options);
            UpdateRerollUI();
        }

        private void UpdateRerollUI()
        {
            int cost = _freeRerolls > 0 ? 0 : _rerollCost;
            _upgradeScreen.SetRerollState(cost, _balanceService.CurrentBalance >= cost);
        }

        private void OnUpgradeSelected(UpgradeStaticData selectedUpgrade)
        {
            _upgradeService.ApplyUpgrade(selectedUpgrade);
            _stateMachine.Enter<GameLoopState>();
        }

    }
}