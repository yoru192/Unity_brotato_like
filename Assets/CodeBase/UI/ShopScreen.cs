using System;
using System.Collections.Generic;
using CodeBase.Infrastructure.Services.Balance;
using CodeBase.StaticData;
using CodeBase.UI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public class ShopScreen : MonoBehaviour
    {
        [SerializeField] private ShopItem shopItemPrefab;
        [SerializeField] private Transform shopItemsContainer;
        [SerializeField] private Button skipButton;
        [SerializeField] private TextMeshProUGUI balanceText;

        private List<ShopItem> _activeShopItems = new List<ShopItem>();
        private Func<ShopItemStaticData, bool> _onTryPurchase;
        private IBalanceService _balanceService;
        private Action _onSkip;

        public void Construct(List<ShopItemStaticData> shopItems,
            Func<ShopItemStaticData, bool> onTryPurchase,
            IBalanceService balanceService,
            Action onSkip)
        {
            _balanceService = balanceService;
            _onTryPurchase = onTryPurchase;
            _onSkip = onSkip;

            _balanceService.OnBalanceChanged += RefreshAffordability;
            skipButton.onClick.AddListener(OnSkipClicked);

            foreach (ShopItemStaticData item in shopItems)
            {
                ShopItem shopItem = Instantiate(shopItemPrefab, shopItemsContainer);
                shopItem.Initialize(item, OnItemClicked, _balanceService.CurrentBalance);
                _activeShopItems.Add(shopItem);
            }

            UpdateBalanceLabel();
        }

        private void OnSkipClicked()
        {
            _onSkip?.Invoke();
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

            UpdateBalanceLabel();
        }

        private void UpdateBalanceLabel()
        {
            if (balanceText != null)
                balanceText.text = $"{_balanceService.CurrentBalance} $";
        }

        private void OnItemClicked(ShopItem shopItem)
        {
            // Buy the item but keep the shop open; mark it sold so it can't be bought twice.
            if (_onTryPurchase != null && _onTryPurchase(shopItem.Data))
                shopItem.MarkSold();
        }


    }
}