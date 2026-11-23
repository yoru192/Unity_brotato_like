using CodeBase.Enemy;
using CodeBase.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Elements
{
    /// <summary>Смуга HP боса вгорі HUD. Прихована, доки бос не з'явиться; ховається, коли він гине.</summary>
    public class BossHealthBar : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [Tooltip("9-sliced заливка: ширина задається якорями, як у смуг HP/стаміни гравця.")]
        [SerializeField] private Image _fill;
        [SerializeField] private TextMeshProUGUI _nameText;

        private IHealth _health;

        private void Awake()
        {
            if (_health == null)
                _root.SetActive(false);
        }

        private void OnDestroy() => Unbind();

        public void Show(GameObject boss)
        {
            Unbind();

            _health = boss.GetComponent<IHealth>();
            if (_health == null) return;

            BossBrain brain = boss.GetComponent<BossBrain>();
            if (_nameText != null && brain != null && brain.Data != null)
                _nameText.text = brain.Data.displayName.ToUpperInvariant();

            _health.HealthChanged += UpdateFill;
            _root.SetActive(true);
            UpdateFill();
        }

        private void Unbind()
        {
            if (_health != null)
                _health.HealthChanged -= UpdateFill;
            _health = null;
        }

        private void UpdateFill()
        {
            float ratio = _health.Max > 0f ? Mathf.Clamp01(_health.Current / _health.Max) : 0f;

            RectTransform rect = _fill.rectTransform;
            rect.anchorMax = new Vector2(ratio, rect.anchorMax.y);
            _fill.enabled = ratio > 0f;

            if (ratio <= 0f)
            {
                Unbind();
                _root.SetActive(false);
            }
        }
    }
}
