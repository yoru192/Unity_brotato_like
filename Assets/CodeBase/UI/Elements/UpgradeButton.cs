using System;
using CodeBase.Data;
using CodeBase.StaticData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Elements
{
    public class UpgradeButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        private UpgradeStaticData _upgradeData;
        private Action<UpgradeStaticData> _onClick;

        private void Awake()
        {
            button.onClick.AddListener(OnClick);
        }

        public void Initialize(UpgradeStaticData upgradeData, Action<UpgradeStaticData> onClick)
        {
            _upgradeData = upgradeData;
            _onClick = onClick;
    
            if (icon != null && upgradeData.icon != null)
                icon.sprite = upgradeData.icon;
        
            if (nameText != null)
                nameText.text = upgradeData.upgradeName;
        
            if (descriptionText != null)
                descriptionText.text = upgradeData.description;
        }
        private void OnClick()
        {
            _onClick?.Invoke(_upgradeData);
        }
    }
}