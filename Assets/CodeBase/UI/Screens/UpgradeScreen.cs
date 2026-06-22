using System;
using System.Collections.Generic;
using CodeBase.StaticData;
using CodeBase.UI.Elements;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Screens
{
    public class UpgradeScreen : MonoBehaviour
    {
        [SerializeField] private UpgradeButton upgradeButtonPrefab;
        [SerializeField] private Transform[] upgradesContainers;

        [Header("Reroll")]
        [SerializeField] private Button rerollButton;
        [SerializeField] private TextMeshProUGUI rerollCostText;

        private List<UpgradeButton> _activeButtons = new List<UpgradeButton>();
        private Action<UpgradeStaticData> _onUpgradeSelected;
        private Action _onReroll;

        private void Awake()
        {
            foreach (MMF_Player player in GetComponentsInChildren<MMF_Player>(true))
            {
                player.ForceTimescaleMode = true;
                player.ForcedTimescaleMode = TimescaleModes.Unscaled;
            }

            if (rerollButton != null)
                rerollButton.onClick.AddListener(OnRerollClicked);
        }

        private void OnDestroy()
        {
            if (rerollButton != null)
                rerollButton.onClick.RemoveListener(OnRerollClicked);
        }

        public void Construct(List<UpgradeStaticData> upgrades, Action<UpgradeStaticData> onUpgradeSelected,
            Action onReroll)
        {
            _onUpgradeSelected = onUpgradeSelected;
            _onReroll = onReroll;
            ShowOptions(upgrades);
        }

        /// <summary>Rebuilds the option buttons. Called on first show and after every reroll.</summary>
        public void ShowOptions(List<UpgradeStaticData> upgrades)
        {
            ClearButtons();

            int count = Mathf.Min(upgrades.Count, upgradesContainers.Length);
            for (int i = 0; i < count; i++)
            {
                UpgradeButton button = Instantiate(upgradeButtonPrefab, upgradesContainers[i]);
                button.Initialize(upgrades[i], OnButtonClicked);
                _activeButtons.Add(button);
            }
        }

        /// <summary>Updates the reroll button label with the current cost and disables it when unaffordable.</summary>
        public void SetRerollState(int cost, bool affordable)
        {
            if (rerollCostText != null)
                rerollCostText.text = cost.ToString();

            if (rerollButton != null)
                rerollButton.interactable = affordable;
        }

        private void OnButtonClicked(UpgradeStaticData upgrade)
        {
            _onUpgradeSelected?.Invoke(upgrade);
        }

        private void OnRerollClicked()
        {
            _onReroll?.Invoke();
        }

        private void ClearButtons()
        {
            foreach (UpgradeButton button in _activeButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }
            _activeButtons.Clear();
        }
    }
}
