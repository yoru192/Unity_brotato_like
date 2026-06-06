using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeBase.Infrastructure.Services.Buff;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.Upgrade;
using CodeBase.StaticData;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CodeBase.Infrastructure.Services.ShopService
{
    public class ShopService : IShopService
    {
        private const float ShopIntervalMinutes = .634f;

        public event Action OnShopTimerTick;

        private readonly IStaticDataService _staticData;
        private readonly IUpgradeService _upgradeService;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly IBuffService _buffService;
        private readonly IPersistentProgressService _persistentProgress;

        private Coroutine _shopTimerCoroutine;

        public ShopService(IStaticDataService staticDataService,
            IUpgradeService upgradeService,
            ICoroutineRunner coroutineRunner,
            IBuffService buffService,
            IPersistentProgressService persistentProgress)
        {
            _staticData = staticDataService;
            _upgradeService = upgradeService;
            _coroutineRunner = coroutineRunner;
            _buffService = buffService;
            _persistentProgress = persistentProgress;
        }

        public void StartShopTimer()
        {
            StopShopTimer();
            _shopTimerCoroutine = _coroutineRunner.StartCoroutine(ShopTimerRoutine());
        }

        public void StopShopTimer()
        {
            if (_shopTimerCoroutine != null)
            {
                _coroutineRunner.StopCoroutine(_shopTimerCoroutine);
                _shopTimerCoroutine = null;
            }
        }

        private IEnumerator ShopTimerRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(ShopIntervalMinutes * 60f);
                OnShopTimerTick?.Invoke();
            }
        }
        
        public List<ShopItemStaticData> GenerateShopItemOptions(int count = 3)
        {
            var ownedWeapons = _persistentProgress.Progress.playerState.OwnedWeapons;
            List<ShopItemStaticData> allShopItems = _staticData.GetAllShopItems()
                .Where(item => item.effect.category != ShopItemCategory.Weapon
                               || !ownedWeapons.Contains(item.effect.weaponTypeId))
                .ToList();

            // Distinct items only — the shop now lets the player buy every offered item, so a
            // duplicate would let the same effect be purchased (and applied) twice.
            List<ShopItemStaticData> selected = new List<ShopItemStaticData>();
            for (int i = 0; i < count && allShopItems.Count > 0; i++)
            {
                ShopItemStaticData item = GetWeightedRandom(allShopItems);
                selected.Add(item);
                allShopItems.Remove(item);
            }

            return selected;
        }

        public void ApplyShopItem(ShopItemStaticData shopItem)
        {
            switch (shopItem.effect.category)
            {
                case ShopItemCategory.Buff:
                    ApplyBuff(shopItem);
                    break;
                case ShopItemCategory.Upgrade:
                    _upgradeService.ApplyUpgrade(ConvertToUpgrade(shopItem));
                    break;
                case ShopItemCategory.Weapon:
                    _persistentProgress.Progress.playerState.OwnedWeapons.Add(shopItem.effect.weaponTypeId);
                    break;
            }
        }

        private void ApplyBuff(ShopItemStaticData shopItem)
        {
            _buffService.ApplyBuff(shopItem.effect.buffType, shopItem.effect.value, shopItem.duration);
        }
        
        private UpgradeStaticData ConvertToUpgrade(ShopItemStaticData shopItem)
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeStaticData>();
            upgrade.modifierType = shopItem.effect.upgradeType;
            upgrade.value = shopItem.effect.value;
            upgrade.weight = shopItem.weight;
            return upgrade;
        }

        private ShopItemStaticData GetWeightedRandom(List<ShopItemStaticData> allShopItems)
        {
            int totalWeight = allShopItems.Sum(i => i.weight);
            int randomValue = Random.Range(0, totalWeight);

            int currentWeight = 0;
            foreach (var shopItem in allShopItems)
            {
                currentWeight += shopItem.weight;
                if (randomValue < currentWeight)
                    return shopItem;
            }

            return allShopItems[0];
        }
    }
}