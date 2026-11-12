using System;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Elements
{
    public class SettingsSwitch : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private GameObject onState;
        [SerializeField] private GameObject offState;

        public event Action<bool> Changed;
        public bool IsOn { get; private set; }

        private void Awake() => button.onClick.AddListener(Toggle);

        private void OnDestroy() => button.onClick.RemoveListener(Toggle);

        public void SetIsOnWithoutNotify(bool value)
        {
            IsOn = value;
            Refresh();
        }

        private void Toggle()
        {
            IsOn = !IsOn;
            Refresh();
            Changed?.Invoke(IsOn);
        }

        private void Refresh()
        {
            onState.SetActive(IsOn);
            offState.SetActive(!IsOn);
        }
    }
}