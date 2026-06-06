using System;
using CodeBase.StaticData;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI
{
    /// <summary>
    /// The campfire overlay (reached from a Campfire node on the map). Two choices: rest to heal, or
    /// take a random upgrade. The upgrade is hidden until taken — picking it rolls, applies it and
    /// presents it on a dimmed reward screen (title + upgrade card), after which the continue button
    /// returns to the map. Resting closes immediately.
    /// </summary>
    public class CampfireScreen : MonoBehaviour
    {
        [Header("Choice Buttons")]
        [SerializeField] private GameObject choiceRoot;
        [SerializeField] private Button restButton;
        [SerializeField] private Button takeUpgradeButton;

        [Header("Reward Screen")]
        [SerializeField] private GameObject rewardScreen;
        [SerializeField] private Image upgradeBackground;
        [SerializeField] private Image upgradeIcon;
        [SerializeField] private TextMeshProUGUI upgradeName;
        [SerializeField] private Button continueButton;
        [SerializeField] private MMF_Player rewardRevealFeedback;

        private Action _onChooseRest;
        private Func<UpgradeStaticData> _onTakeUpgrade;
        private Action _onContinue;

        private void Awake()
        {
            foreach (MMF_Player player in GetComponentsInChildren<MMF_Player>(true))
            {
                player.ForceTimescaleMode = true;
                player.ForcedTimescaleMode = TimescaleModes.Unscaled;
            }

            if (restButton != null)
                restButton.onClick.AddListener(OnRestClicked);
            if (takeUpgradeButton != null)
                takeUpgradeButton.onClick.AddListener(OnTakeUpgradeClicked);
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);
        }

        private void OnDestroy()
        {
            if (restButton != null)
                restButton.onClick.RemoveListener(OnRestClicked);
            if (takeUpgradeButton != null)
                takeUpgradeButton.onClick.RemoveListener(OnTakeUpgradeClicked);
            if (continueButton != null)
                continueButton.onClick.RemoveListener(OnContinueClicked);
        }

        public void Construct(Action onChooseRest, Func<UpgradeStaticData> onTakeUpgrade, Action onContinue)
        {
            _onChooseRest = onChooseRest;
            _onTakeUpgrade = onTakeUpgrade;
            _onContinue = onContinue;
            
            if (choiceRoot != null)
                choiceRoot.SetActive(true);
            if (rewardScreen != null)
                rewardScreen.SetActive(false);
        }

        private void OnRestClicked()
        {
            _onChooseRest?.Invoke();
            _onContinue?.Invoke();
        }

        private void OnTakeUpgradeClicked()
        {
            UpgradeStaticData rolled = _onTakeUpgrade?.Invoke();
            ShowRewardScreen(rolled);
        }

        private void OnContinueClicked()
        {
            _onContinue?.Invoke();
        }
        
        private void ShowRewardScreen(UpgradeStaticData upgrade)
        {
            if (choiceRoot != null)
                choiceRoot.SetActive(false);

            if (upgrade != null)
            {
                if (upgradeBackground != null && upgrade.background != null)
                    upgradeBackground.sprite = upgrade.background;
                if (upgradeIcon != null && upgrade.icon != null)
                    upgradeIcon.sprite = upgrade.icon;
                if (upgradeName != null)
                    upgradeName.text = upgrade.upgradeName;
            }

            if (rewardScreen != null)
                rewardScreen.SetActive(true);
            
            if (rewardRevealFeedback != null)
                rewardRevealFeedback.PlayFeedbacks();
        }
    }
}
