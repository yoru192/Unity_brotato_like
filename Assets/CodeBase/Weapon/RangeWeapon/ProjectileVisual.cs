using UnityEngine;

namespace CodeBase.Weapon.RangeWeapon
{
    public class ProjectileVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _sprite;
        [SerializeField] private Transform _shadow;

        [Header("Shadow")]
        [Tooltip("Масштаб тіні коли снаряд на землі (t=0 або t=1).")]
        [SerializeField] private float _shadowMaxScale = 1f;
        [Tooltip("Масштаб тіні на піку дуги (t=0.5).")]
        [SerializeField] private float _shadowMinScale = 0.3f;

        private float _arcHeight;
        private Vector3 _lastRealPosition;
        public void Initialize(float arcHeight)
        {
            _arcHeight = arcHeight;
            _lastRealPosition = transform.position;
        }
        public void UpdateArc(float t, Vector3 realPosition, Vector3 flightDirection)
        {
            float arcOffset = Mathf.Sin(t * Mathf.PI) * _arcHeight;
            
            _sprite.position = realPosition + new Vector3(0f, arcOffset, 0f);

            _shadow.position = realPosition;
            
            Vector3 horizontalDelta = realPosition - _lastRealPosition;
            if (horizontalDelta.sqrMagnitude > 0.00001f)
            {
                float angle = Mathf.Atan2(horizontalDelta.y, horizontalDelta.x) * Mathf.Rad2Deg;
                _sprite.rotation = Quaternion.Euler(0f, 0f, angle);
            }
            
            float shadowScale = Mathf.Lerp(_shadowMaxScale, _shadowMinScale, Mathf.Sin(t * Mathf.PI));
            _shadow.localScale = new Vector3(shadowScale, shadowScale, 1f);

            _lastRealPosition = realPosition;
        }
    }
}