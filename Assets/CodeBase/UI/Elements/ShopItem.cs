using System;
using CodeBase.StaticData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace CodeBase.UI.Elements
{
    public class ShopItem : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI price;
        
        private ShopItemStaticData _itemData;
        private Action<ShopItemStaticData> _onItemClicked;

        
        private void Awake()
        {
            button.onClick.AddListener(OnClick);
        }

        public void Initialize(ShopItemStaticData itemData, Action<ShopItemStaticData> onItemClicked, int currentBalance)
        {
            _itemData = itemData;
            _onItemClicked = onItemClicked;
        
            if (background != null && itemData.itemBackground != null)
                background.sprite = itemData.itemBackground;
        
            if (icon != null && itemData.itemIcon != null)
                icon.sprite = itemData.itemIcon;
        
            if (nameText != null)
                nameText.text = itemData.itemName;
        
            if (descriptionText != null)
                descriptionText.text = itemData.itemDescription;
        
            if (price != null)
                price.text = $"{itemData.itemPrice} $";
        
            UpdateAffordability(currentBalance);
        }
        
        public void UpdateAffordability(int currentBalance)
        {
            if (price == null) return;
            price.color = currentBalance >= _itemData.itemPrice ? Color.white : Color.red;
        }
        private void OnClick()
        {
            _onItemClicked?.Invoke(_itemData);
        }
    }
}