using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Elements
{
    public class StaminaBar : MonoBehaviour
    {
        public Image imageCurrent;

        public void SetValue(float current, float max) =>
            imageCurrent.fillAmount = current / max;
    }
}