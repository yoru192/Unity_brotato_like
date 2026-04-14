using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.Logic
{
    public class LoadingCurtain : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private CanvasGroup _curtain;

        [Header("Progress Bar")]
        [SerializeField] private Slider _progressBarSlider;
        [SerializeField] private TextMeshProUGUI _progressBarText;
        [SerializeField] private float _progressSmoothing = 4f;

        [Header("Spinner")]
        [SerializeField] private GameObject _loadingCircle;

        private float _targetProgress;
        private bool _isRunning;

        private void Awake() =>
            DontDestroyOnLoad(this);

        private void Update()
        {
            if (!_isRunning || _progressBarSlider == null) return;
            
            _progressBarSlider.value = Mathf.MoveTowards(
                _progressBarSlider.value,
                _targetProgress,
                Time.deltaTime * _progressSmoothing
            );
            _progressBarText.text =$"Loading... {((int)_progressBarSlider.value * 100).ToString()}%";
        }

        public void Show()
        {
            gameObject.SetActive(true);
            _curtain.alpha = 1f;
            _targetProgress = 0f;

            if (_progressBarSlider != null)
                _progressBarSlider.value = 0f;
            
            if (_loadingCircle != null)
                _loadingCircle.SetActive(true);

            _isRunning = true;
        }

        public void SetProgress(float progress)
        {
            _targetProgress = Mathf.Clamp01(progress);
            Debug.Log($"[Curtain] SetProgress called: {progress:F2}");
        }

        public void Hide() =>
            StartCoroutine(FinishAndFadeOut());

        private IEnumerator FinishAndFadeOut()
        {
            _targetProgress = 1f;
            
            while (_progressBarSlider != null && _progressBarSlider.value < 0.99f)
                yield return null;

            yield return new WaitForSeconds(0.4f);

            if (_loadingCircle != null)
                _loadingCircle.SetActive(false);

            _isRunning = false;

            float duration = 0.5f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _curtain.alpha = 1f - (elapsed / duration);
                yield return null;
            }

            _curtain.alpha = 0f;
            gameObject.SetActive(false);
        }
    }
}