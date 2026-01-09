using System;
using System.Collections.Generic;
using CodeBase.Data;
using CodeBase.StaticData;
using CodeBase.UI.Elements;
using UnityEngine;

namespace CodeBase.UI
{
    public class UpgradeScreen : MonoBehaviour
    {
        [SerializeField] private UpgradeButton upgradeButtonPrefab;
        [SerializeField] private Transform upgradesContainer;

        private List<UpgradeButton> _activeButtons = new List<UpgradeButton>();
        private Action<UpgradeStaticData> _onUpgradeSelected;

        public void Construct(List<UpgradeStaticData> upgrades, Action<UpgradeStaticData> onUpgradeSelected)
        {
            _onUpgradeSelected = onUpgradeSelected;
            ClearButtons();
    
            foreach (UpgradeStaticData upgrade in upgrades)
            {
                UpgradeButton button = Instantiate(upgradeButtonPrefab, upgradesContainer);
                button.Initialize(upgrade, OnButtonClicked);
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
                Destroy(button.gameObject);
            }
            _activeButtons.Clear();
        }
    }
}