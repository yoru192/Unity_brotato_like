using System;
using System.Collections.Generic;
using CodeBase.Infrastructure.Services.Balance;
using CodeBase.Infrastructure.States;
using CodeBase.StaticData;
using CodeBase.UI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public class ShopScreen : MonoBehaviour
    {
        [SerializeField] private ShopItem shopItemPrefab;
        [SerializeField] private Transform shopItemsContainer;
        [SerializeField] private Button skipButton;

        private List<ShopItem> _activeShopItems = new List<ShopItem>();
        private Action<ShopItemStaticData> _onShopItemSelected;
        private IBalanceService _balanceService;
        private IGameStateMachine _gameStateMachine;

        public void Construct(List<ShopItemStaticData> shopItems,
            Action<ShopItemStaticData> onShopItemSelected,
            IBalanceService balanceService,
            IGameStateMachine gameStateMachine)
        {
            _balanceService = balanceService;
            _onShopItemSelected = onShopItemSelected;
            _gameStateMachine = gameStateMachine;

            _balanceService.OnBalanceChanged += RefreshAffordability;
            skipButton.onClick.AddListener(OnSkipClicked);

            foreach (ShopItemStaticData item in shopItems)
            {
                ShopItem shopItem = Instantiate(shopItemPrefab, shopItemsContainer);
                shopItem.Initialize(item, OnItemClicked, _balanceService.CurrentBalance);
                _activeShopItems.Add(shopItem);
            }
        }

        private void OnSkipClicked()
        {
            _gameStateMachine.Enter<GameLoopState>();
        }

        private void OnDestroy()
        {
            if (_balanceService != null)
                _balanceService.OnBalanceChanged -= RefreshAffordability;
        }

        private void RefreshAffordability()
        {
            foreach (ShopItem item in _activeShopItems)
                item.UpdateAffordability(_balanceService.CurrentBalance);
        }

        private void OnItemClicked(ShopItemStaticData shopItem)
        {
            _onShopItemSelected?.Invoke(shopItem);
        }
        
        
    }
}