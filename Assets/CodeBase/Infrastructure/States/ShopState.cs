using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.Balance;
using CodeBase.Infrastructure.Services.Map;
using CodeBase.Infrastructure.Services.ShopService;
using CodeBase.StaticData;
using CodeBase.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeBase.Infrastructure.States
{
    /// <summary>
    /// The shop, now reachable only from a Shop node on the level map (no in-run timer). Opens as an
    /// overlay in the map scene; purchases are written to progress and closing returns to the map.
    /// </summary>
    public class ShopState : IState
    {
        private readonly IShopService _shopService;
        private readonly IGameFactory _gameFactory;

        private GameObject _shopUI;
        private readonly IGameStateMachine _gameStateMachine;
        private readonly IBalanceService _balanceService;
        private readonly IMapService _mapService;

        public ShopState(IShopService shopService,
            IGameFactory gameFactory,
            IGameStateMachine gameStateMachine,
            IBalanceService balanceService,
            IMapService mapService)
        {
            _shopService = shopService;
            _gameFactory = gameFactory;
            _gameStateMachine = gameStateMachine;
            _balanceService = balanceService;
            _mapService = mapService;
        }
        
        public void Enter()
        {
            Time.timeScale = 0f;
            _ = ShowShopItemsOptions();
        }

        public void Exit()
        {
            Time.timeScale = 1f;
            if (_shopUI != null)
                Object.Destroy(_shopUI);
        }

        private async Task ShowShopItemsOptions()
        {
            try
            {
                List<ShopItemStaticData> options = _shopService.GenerateShopItemOptions(4);
                await CreateShopUI(options);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async Task CreateShopUI(List<ShopItemStaticData> options)
        {
            _shopUI = await _gameFactory.CreateShopScreen();
            
            ShopScreen shopScreen = _shopUI.GetComponentInChildren<ShopScreen>();
            if (shopScreen != null)
            {
                shopScreen.Construct(options, TryPurchase, _balanceService, CloseToMap);
            }
            else
            {
                Debug.LogError("ShopScreen component not found!");
            }
        }

        /// <summary>
        /// Attempts to buy a single item. The shop stays open so the player can keep buying until
        /// they leave via the skip/close button. Returns true when the purchase went through.
        /// </summary>
        private bool TryPurchase(ShopItemStaticData selectedItem)
        {
            if (!_balanceService.TryRemoveBalance(selectedItem.itemPrice))
                return false;

            // Records the purchase in progress (incl. weapons into OwnedWeapons). Weapons are
            // equipped lazily when the next combat level spawns the player — there is no player here.
            _shopService.ApplyShopItem(selectedItem);
            return true;
        }

        private void CloseToMap()
        {
            _mapService.CompleteSelectedNode();
            _gameStateMachine.Enter<LevelMapState>();
        }
    }
}