using System;
using CodeBase.StaticData.Weapon;

namespace CodeBase.StaticData
{
    [Serializable]
    public class ShopItemEffect
    {
        public ShopItemCategory category;
        public float value;

        public BuffType buffType;
        public StatModifierType upgradeType;
        public WeaponTypeId weaponTypeId;
    }
}