using System;
using System.Collections.Generic;
using CodeBase.StaticData;
using CodeBase.UI.Elements;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace CodeBase.UI
{
    public class UpgradeScreen : MonoBehaviour
    {
        [SerializeField] private UpgradeButton upgradeButtonPrefab;
        [SerializeField] private Transform[] upgradesContainers;

        private List<UpgradeButton> _activeButtons = new List<UpgradeButton>();
        private Action<UpgradeStaticData> _onUpgradeSelected;

        private void Awake()
        {
            foreach (MMF_Player player in GetComponentsInChildren<MMF_Player>(true))
            {
                player.ForceTimescaleMode = true;
                player.ForcedTimescaleMode = TimescaleModes.Unscaled;
            }
        }

        public void Construct(List<UpgradeStaticData> upgrades, Action<UpgradeStaticData> onUpgradeSelected)
        {
            _onUpgradeSelected = onUpgradeSelected;
            ClearButtons();

            int count = Mathf.Min(upgrades.Count, upgradesContainers.Length);
            for (int i = 0; i < count; i++)
            {
                UpgradeButton button = Instantiate(upgradeButtonPrefab, upgradesContainers[i]);
                button.Initialize(upgrades[i], OnButtonClicked);
                _activeButtons.Add(button);
            }
        }

        private void OnButtonClicked(UpgradeStaticData upgrade)
        {
            _onUpgradeSelected?.Invoke(upgrade);
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