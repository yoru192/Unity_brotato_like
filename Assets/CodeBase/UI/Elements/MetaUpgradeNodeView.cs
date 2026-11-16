using System;
using CodeBase.Infrastructure.Services.MetaProgress;
using CodeBase.StaticData.MetaUpgrade;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Elements
{
    /// <summary>Один вузол дерева мета-апгрейдів: назва, опис, рівень, ціна та кнопка купівлі.</summary>
    public class MetaUpgradeNodeView : MonoBehaviour
    {
        [SerializeField] private Button buyButton;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [Tooltip("Куплений рівень у форматі \"2/5\".")]
        [SerializeField] private TextMeshProUGUI levelText;
        [Tooltip("Ціна наступного рівня, MAX або вимога гілки для закритого вузла.")]
        [SerializeField] private TextMeshProUGUI costText;
        [Tooltip("Опційно: об'єкт, що вмикається, поки вузол закритий вимогою гілки.")]
        [SerializeField] private GameObject lockedOverlay;

        private MetaUpgradeStaticData _data;
        private Action<MetaUpgradeStaticData> _onBuy;

        private void Awake()
        {
            if (buyButton != null)
                buyButton.onClick.AddListener(OnBuyClicked);
        }

        private void OnDestroy()
        {
            if (buyButton != null)
                buyButton.onClick.RemoveListener(OnBuyClicked);
        }

        public void Initialize(MetaUpgradeStaticData data, Action<MetaUpgradeStaticData> onBuy)
        {
            _data = data;
            _onBuy = onBuy;

            if (icon != null && data.icon != null)
                icon.sprite = data.icon;

            if (nameText != null)
                nameText.text = data.upgradeName;

            if (descriptionText != null)
                descriptionText.text = data.description;
        }

        public void Refresh(IMetaProgressService metaProgress)
        {
            if (_data == null) return;

            int level = metaProgress.GetUpgradeLevel(_data.id);
            bool unlocked = metaProgress.IsUpgradeUnlocked(_data);
            int cost = _data.CostForNextLevel(level);

            if (levelText != null)
                levelText.text = $"{level}/{_data.MaxLevel}";

            if (costText != null)
            {
                if (cost < 0)
                    costText.text = "MAX";
                else if (!unlocked)
                    costText.text = $"Need {_data.requiredBranchLevels} levels in branch";
                else
                    costText.text = cost.ToString();
            }

            if (lockedOverlay != null)
                lockedOverlay.SetActive(!unlocked);

            if (buyButton != null)
                buyButton.interactable = metaProgress.CanBuyUpgrade(_data);
        }

        private void OnBuyClicked()
        {
            _onBuy?.Invoke(_data);
        }
    }
}
