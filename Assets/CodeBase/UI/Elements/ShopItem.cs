using System;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Audio;
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
        private Action<ShopItem> _onItemClicked;
        private bool _sold;
        private IAudioService _audioService;

        public ShopItemStaticData Data => _itemData;

        private void Awake()
        {
            _audioService = AllServices.Container.Single<IAudioService>();
            button.onClick.AddListener(OnClick);
        }

        public void Initialize(ShopItemStaticData itemData, Action<ShopItem> onItemClicked, int currentBalance)
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
            if (_sold) return;

            bool affordable = currentBalance >= _itemData.itemPrice;
            if (price != null)
                price.color = affordable ? Color.white : Color.red;
            if (button != null)
                button.interactable = affordable;
        }

        /// <summary>Marks the item as bought: disables the button and shows it as sold.</summary>
        public void MarkSold()
        {
            _sold = true;
            if (button != null)
                button.interactable = false;
            if (price != null)
            {
                price.text = "SOLD";
                price.color = Color.gray;
            }
        }

        private void OnClick()
        {
            _audioService.PlaySfx(AudioClipId.ShopItemPick);
            _onItemClicked?.Invoke(this);
        }
    }
}