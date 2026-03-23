using System.Collections;
using UnityEngine;
using Pathfinding;

namespace CodeBase.Enemy
{
    public class AttackIndicator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _indicatorRenderer;
        [SerializeField] private SpriteRenderer _fillRenderer;
        [SerializeField] private Transform _maskTransform;

        [Header("Межі маски (локальні X)")]
        [SerializeField] private float _maskStartX = -2f;
        [SerializeField] private float _maskEndX   =  2f;

        [Header("Колір")]
        [SerializeField] private Color _windupColor = new Color(1f, 0.16f, 0.16f, 1f);

        private Coroutine _activeCoroutine;
        private FollowerEntity _followerEntity;
        private Vector3 _baseScale;
        [SerializeField] private float windupDuration = 0.7f;

        private void Awake()
        {
            _followerEntity = GetComponentInParent<FollowerEntity>();
            _baseScale = transform.localScale;
            ResetState();
        }

        private void Update()
        {
            if (_followerEntity == null) return;
            if (_followerEntity.velocity.x >= 0.01f)
                transform.localScale = new Vector3(Mathf.Abs(_baseScale.x), _baseScale.y, _baseScale.z);
            else if (_followerEntity.velocity.x <= -0.01f)
                transform.localScale = new Vector3(-Mathf.Abs(_baseScale.x), _baseScale.y, _baseScale.z);
        }

        public void ShowWindup()
        {
            StopActive();
            _activeCoroutine = StartCoroutine(WindupRoutine(windupDuration));
        }

        public void ShowStrike()
        {
            StopActive();
            _activeCoroutine = StartCoroutine(StrikeRoutine());
        }

        public void Hide() => ResetState();

        public void ResetState()
        {
            StopActive();
            _indicatorRenderer.enabled = false;
            _fillRenderer.enabled      = false;
            SetMaskProgress(0f);
        }

        private IEnumerator WindupRoutine(float duration)
        {
            _indicatorRenderer.color   = _windupColor;
            _fillRenderer.color        = _windupColor;
            _indicatorRenderer.enabled = true;
            _fillRenderer.enabled      = true;
            SetMaskProgress(0f);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                SetMaskProgress(Mathf.Clamp01(elapsed / duration));
                yield return null;
            }
        }

        private IEnumerator StrikeRoutine()
        {
            _indicatorRenderer.enabled = true;
            _fillRenderer.enabled      = true;
            SetMaskProgress(1f);

            yield return new WaitForSeconds(0.18f);

            ResetState();
        }

        private void SetMaskProgress(float progress)
        {
            if (_maskTransform == null) return;
            var pos = _maskTransform.localPosition;
            pos.x = Mathf.Lerp(_maskStartX, _maskEndX, Mathf.Clamp01(progress));
            _maskTransform.localPosition = pos;
        }

        private void StopActive()
        {
            if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
            _activeCoroutine = null;
        }
    }
}