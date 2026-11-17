using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.LoadoutSelect
{
    public class StatBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [Tooltip("Optional: the actual stat value shown next to the bar.")]
        [SerializeField] private TMP_Text valueText;

        public void SetValue(float value)
        {
            float clamped = Mathf.Clamp01(value);
            if (slider) slider.value = clamped;
        }

        public void SetValue(float normalized, string label)
        {
            SetValue(normalized);
            if (valueText) valueText.text = label;
        }
    }
}
