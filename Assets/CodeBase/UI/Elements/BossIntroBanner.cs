using System.Collections;
using TMPro;
using UnityEngine;

namespace CodeBase.UI.Elements
{
    /// <summary>Велика назва боса по центру екрана: з'являється, тримається і зникає (unscaled time).</summary>
    public class BossIntroBanner : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private float _fadeIn = 0.35f;
        [SerializeField] private float _hold = 1.4f;
        [SerializeField] private float _fadeOut = 0.5f;

        private Coroutine _routine;

        private void Awake()
        {
            _group.alpha = 0f;
            _group.gameObject.SetActive(false);
        }

        public void Play(string title)
        {
            _title.text = title.ToUpperInvariant();

            if (_routine != null)
                StopCoroutine(_routine);
            _routine = StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            _group.gameObject.SetActive(true);

            for (float t = 0f; t < _fadeIn; t += Time.unscaledDeltaTime)
            {
                _group.alpha = t / _fadeIn;
                yield return null;
            }
            _group.alpha = 1f;

            yield return new WaitForSecondsRealtime(_hold);

            for (float t = 0f; t < _fadeOut; t += Time.unscaledDeltaTime)
            {
                _group.alpha = 1f - t / _fadeOut;
                yield return null;
            }

            _group.alpha = 0f;
            _group.gameObject.SetActive(false);
            _routine = null;
        }
    }
}
