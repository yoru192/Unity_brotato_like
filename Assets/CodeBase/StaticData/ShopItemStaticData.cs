using UnityEngine;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(menuName = "StaticData/ShopItem")]
    public class ShopItemStaticData : ScriptableObject
    {
        [Header("UI Info")] 
        public string itemName;
        public string itemDescription;
        public Sprite itemIcon;
        public Sprite itemBackground;

        public int itemPrice;

        [Header("Effect")] 
        public ShopItemEffect effect;
        public float duration;

        [Header("Optional")] 
        public int weight = 1;
    }

}

    public enum ShopItemCategory { Buff, Upgrade, Weapon }
    public enum BuffType { MaxHealth, MoveSpeed, Damage, }