using System;
using System.Collections.Generic;
using CodeBase.StaticData;

namespace CodeBase.Infrastructure.Services.ShopService
{
    public interface IShopService : IService
    {
        List<ShopItemStaticData> GenerateShopItemOptions(int count = 6);
        void ApplyShopItem(ShopItemStaticData shopItem);
        event Action OnShopTimerTick;
        void StartShopTimer();
        void StopShopTimer();
    }
}