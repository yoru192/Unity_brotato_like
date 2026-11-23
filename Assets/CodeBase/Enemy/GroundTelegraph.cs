using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Enemy
{
    /// <summary>
    /// Попередження на землі: контур кола одразу показує зону удару, а заливка росте від центру до краю
    /// за час попередження. Лише візуал — урон наносить той, хто його показав. Повертається в пул сам.
    /// </summary>
    public class GroundTelegraph : MonoBehaviour
    {
        [Tooltip("Статична напівпрозора підкладка на всю зону удару — видно площу одразу.")]
        [SerializeField] private SpriteRenderer _area;
        [SerializeField] private SpriteRenderer _fill;
        [SerializeField] private LineRenderer _ring;
        [SerializeField] private int _ringSegments = 48;
        [SerializeField] private float _ringWidth = 0.14f;
        [Range(0f, 1f)]
        [SerializeField] private float _areaAlphaFactor = 0.4f;

        private float _duration;
        private float _elapsed;
        private float _radius;
        private float _fillSpriteDiameter = 1f;

        public void Show(float radius, float duration, Color color)
        {
            _radius = radius;
            _duration = Mathf.Max(0.01f, duration);
            _elapsed = 0f;

            if (_fill != null)
            {
                if (_fill.sprite != null)
                    _fillSpriteDiameter = Mathf.Max(0.01f, _fill.sprite.bounds.size.x);
                _fill.color = color;
                _fill.transform.localScale = Vector3.zero;
            }

            if (_area != null)
            {
                Color areaColor = color;
                areaColor.a *= _areaAlphaFactor;
                _area.color = areaColor;
                float areaScale = _radius * 2f / Mathf.Max(0.01f, _area.sprite != null ? _area.sprite.bounds.size.x : 1f);
                _area.transform.localScale = new Vector3(areaScale, areaScale, 1f);
            }

            if (_ring != null)
            {
                Color ringColor = color;
                ringColor.a = 1f;
                _ring.startColor = ringColor;
                _ring.endColor = ringColor;
                _ring.startWidth = _ringWidth;
                _ring.endWidth = _ringWidth;
                BuildRing();
            }
        }

        /// <summary>Прибрати достроково (бос загинув до падіння яйця).</summary>
        public void Hide()
        {
            if (gameObject.activeInHierarchy)
                ObjectPoolManager.ReturnObjectToPool(gameObject, ObjectPoolManager.PoolType.Projectile);
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(_elapsed / _duration);

            if (_fill != null)
            {
                float scale = progress * _radius * 2f / _fillSpriteDiameter;
                _fill.transform.localScale = new Vector3(scale, scale, 1f);
            }

            if (progress >= 1f)
                Hide();
        }

        private void BuildRing()
        {
            _ring.useWorldSpace = false;
            _ring.loop = true;
            _ring.positionCount = _ringSegments;

            float step = Mathf.PI * 2f / _ringSegments;
            for (int i = 0; i < _ringSegments; i++)
            {
                float angle = step * i;
                _ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * _radius, Mathf.Sin(angle) * _radius, 0f));
            }
        }
    }
}
