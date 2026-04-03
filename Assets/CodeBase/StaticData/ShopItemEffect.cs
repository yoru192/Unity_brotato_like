using System;

namespace CodeBase.StaticData
{
    [Serializable]
    public class ShopItemEffect
    {
        public ShopItemCategory category;
        public float value;
        
        public BuffType buffType;
        public StatModifierType upgradeType;
 
    }
}