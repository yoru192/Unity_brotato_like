using UnityEngine;

namespace CodeBase.Infrastructure.Services.Vfx
{
    /// <summary>
    /// Керує одним екземпляром ефекту з пулу: слідує за ціллю та/або вимикає себе після lifetime.
    /// Вимкнений (неактивний) екземпляр VfxService бере повторно.
    /// </summary>
    public class VfxInstance : MonoBehaviour
    {
        private Transform _target;
        private bool _follow;
        private Vector3 _offset;
        private float _lifetimeLeft;
        private bool _hasLifetime;

        public void Setup(Transform target, Vector3 offset, float lifetime)
        {
            _target = target;
            _follow = target != null;
            _offset = offset;
            _hasLifetime = lifetime > 0f;
            _lifetimeLeft = lifetime;
        }

        private void LateUpdate()
        {
            if (_follow)
            {
                // Ціль зникла або повернулась у пул — ефекту більше нема за ким слідувати.
                if (_target == null || !_target.gameObject.activeInHierarchy)
                {
                    gameObject.SetActive(false);
                    return;
                }

                transform.position = _target.position + _offset;
            }

            if (_hasLifetime)
            {
                _lifetimeLeft -= Time.deltaTime;
                if (_lifetimeLeft <= 0f)
                    gameObject.SetActive(false);
            }
        }
    }
}
