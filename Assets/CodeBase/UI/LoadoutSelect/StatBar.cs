using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.LoadoutSelect
{
    public class StatBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        
        public void SetValue(float value)
        {
            float clamped = Mathf.Clamp01(value);
            if (slider) slider.value = clamped;
        }
    }
}