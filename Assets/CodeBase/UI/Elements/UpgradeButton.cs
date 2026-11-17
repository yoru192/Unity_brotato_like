using System;
using CodeBase.StaticData;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Elements
{
    public class UpgradeButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("Frame by kind")]
        [Tooltip("Shown for perks (purple frame, corner decor, PERK tag).")]
        [SerializeField] private GameObject perkFrame;
        [Tooltip("Shown for stat upgrades (plain grey frame).")]
        [SerializeField] private GameObject statFrame;

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
            
            if (background != null && upgradeData.background != null)
                background.sprite = upgradeData.background;
            
            if (icon != null && upgradeData.icon != null)
                icon.sprite = upgradeData.icon;
        
            if (nameText != null)
                nameText.text = upgradeData.upgradeName;
        
            if (descriptionText != null)
                descriptionText.text = upgradeData.description;

            bool isPerk = upgradeData.kind == UpgradeKind.Perk;
            if (perkFrame != null)
                perkFrame.SetActive(isPerk);
            if (statFrame != null)
                statFrame.SetActive(!isPerk);

            MMTwoSidedUI twoSided = GetComponent<MMTwoSidedUI>();
            if (twoSided != null)
            {
                twoSided.Back.gameObject.SetActive(false);
                twoSided.Front.gameObject.SetActive(true);
            }
        }
        private void OnClick()
        {
            _onClick?.Invoke(_upgradeData);
        }
    }
}