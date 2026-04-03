using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.Balance;
using CodeBase.Infrastructure.Services.ShopService;
using CodeBase.StaticData;
using CodeBase.UI;
using UnityEngine;

namespace CodeBase.Infrastructure.States
{
    public class ShopState : IState
    {
        private readonly IShopService _shopService;
        private readonly IGameFactory _gameFactory;

        private GameObject _shopUI;
        private readonly IGameStateMachine _gameStateMachine;
        private readonly IBalanceService _balanceService;

        public ShopState(IShopService shopService,
            IGameFactory gameFactory,
            IGameStateMachine gameStateMachine,
            IBalanceService balanceService)
        {
            _shopService = shopService;
            _gameFactory = gameFactory;
            _gameStateMachine = gameStateMachine;
            _balanceService = balanceService;
        }
        
        public void Enter()
        {
            Time.timeScale = 0f;
            ShowShopItemsOptions();
        }

        public void Exit()
        {
            Time.timeScale = 1f;
            if (_shopUI != null)
                Object.Destroy(_shopUI);
        }

        private async void ShowShopItemsOptions()
        {
            List<ShopItemStaticData> options = _shopService.GenerateShopItemOptions(4);
            await CreateShopUI(options);
            
            if (_shopUI == null) return;
        }

        private async Task CreateShopUI(List<ShopItemStaticData> options)
        {
            _shopUI = await _gameFactory.CreateShopScreen();
            
            ShopScreen shopScreen = _shopUI.GetComponentInChildren<ShopScreen>();
            if (shopScreen != null)
            {
                shopScreen.Construct(options, OnShopItemSelected, _balanceService, _gameStateMachine);
            }
            else
            {
                Debug.LogError("ShopScreen component not found!");
            }
        }
        
        private void OnShopItemSelected(ShopItemStaticData selectedUpgrade)
        {
            if (!_balanceService.TryRemoveBalance(selectedUpgrade.itemPrice))
            {
                return;
            }

            _shopService.ApplyShopItem(selectedUpgrade);
            _gameStateMachine.Enter<GameLoopState>();
        }
    }
}