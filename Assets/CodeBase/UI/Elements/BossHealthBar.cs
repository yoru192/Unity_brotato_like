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
        [Tooltip("Мітка на смузі там, де бос переходить у другу фазу.")]
        [SerializeField] private RectTransform _phaseMarker;

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

            if (_phaseMarker != null)
            {
                bool hasPhase = brain != null && brain.Data != null && brain.Data.phase2HealthFraction > 0f;
                _phaseMarker.gameObject.SetActive(hasPhase);
                if (hasPhase)
                {
                    float x = brain.Data.phase2HealthFraction;
                    _phaseMarker.anchorMin = new Vector2(x, _phaseMarker.anchorMin.y);
                    _phaseMarker.anchorMax = new Vector2(x, _phaseMarker.anchorMax.y);
                }
            }

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
