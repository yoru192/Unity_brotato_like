using UnityEngine;

namespace CodeBase.CameraLogic
{
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

        [Header("Bounds (опційно)")]
        [SerializeField] private bool useBounds = true;
        [SerializeField] private Vector2 boundsCenter = Vector2.zero;
        [SerializeField] private Vector2 boundsSize = new Vector2(60f, 34f);

        private Camera cam;
        private float _shakeAmplitude;
        private float _shakeDuration;
        private float _shakeTimer;

        private void Awake() => cam = GetComponent<Camera>();

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 pos = target.position + offset;

            if (useBounds)
                pos = ClampToBounds(pos);

            transform.position = pos + ShakeOffset();
        }

        /// <summary>Тряска камери, що згасає за duration секунд (unscaled). Сильніша тряска не перебивається слабшою.</summary>
        public void Shake(float amplitude, float duration)
        {
            if (amplitude <= 0f || duration <= 0f) return;

            float remainingAmplitude = _shakeTimer > 0f ? _shakeAmplitude * (_shakeTimer / _shakeDuration) : 0f;
            if (amplitude < remainingAmplitude) return;

            _shakeAmplitude = amplitude;
            _shakeDuration = duration;
            _shakeTimer = duration;
        }

        private Vector3 ShakeOffset()
        {
            if (_shakeTimer <= 0f) return Vector3.zero;

            _shakeTimer -= Time.unscaledDeltaTime;
            if (_shakeTimer <= 0f) return Vector3.zero;

            return Random.insideUnitCircle * (_shakeAmplitude * (_shakeTimer / _shakeDuration));
        }

        public void Follow(GameObject following) => target = following.transform;

        /// <summary>Clamps the camera to the given arena (overrides the inspector bounds).</summary>
        public void SetBounds(Rect bounds)
        {
            useBounds = true;
            boundsCenter = bounds.center;
            boundsSize = bounds.size;
        }

        private Vector3 ClampToBounds(Vector3 pos)
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;

            float minX = boundsCenter.x - boundsSize.x * 0.5f + halfW;
            float maxX = boundsCenter.x + boundsSize.x * 0.5f - halfW;
            float minY = boundsCenter.y - boundsSize.y * 0.5f + halfH;
            float maxY = boundsCenter.y + boundsSize.y * 0.5f - halfH;

            pos.x = minX > maxX ? boundsCenter.x : Mathf.Clamp(pos.x, minX, maxX);
            pos.y = minY > maxY ? boundsCenter.y : Mathf.Clamp(pos.y, minY, maxY);
            return pos;
        }

        private void OnDrawGizmosSelected()
        {
            if (!useBounds) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(boundsCenter, boundsSize);
        }
    }
}