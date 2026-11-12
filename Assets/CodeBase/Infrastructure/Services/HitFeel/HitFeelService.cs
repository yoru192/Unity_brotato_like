using System.Collections;
using CodeBase.CameraLogic;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.HitFeel
{
    public class HitFeelService : IHitFeelService
    {
        private const float HitStopTimeScale = 0.05f;

        private readonly ICoroutineRunner _coroutineRunner;
        private CameraFollow _cameraFollow;
        private bool _isHitStopActive;

        public HitFeelService(ICoroutineRunner coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
        }

        public void Shake(float amplitude, float duration)
        {
            if (amplitude <= 0f || duration <= 0f) return;

            CameraFollow cameraFollow = GetCameraFollow();
            if (cameraFollow != null)
                cameraFollow.Shake(amplitude, duration);
        }

        public void HitStop(float duration)
        {
            if (duration <= 0f || _isHitStopActive) return;

            // Не чіпаємо timeScale, якщо ним уже керує інший стан (пауза, магазин, апгрейд тощо).
            if (!Mathf.Approximately(Time.timeScale, 1f)) return;

            _coroutineRunner.StartCoroutine(HitStopRoutine(duration));
        }

        private IEnumerator HitStopRoutine(float duration)
        {
            _isHitStopActive = true;
            Time.timeScale = HitStopTimeScale;

            float elapsed = 0f;
            while (elapsed < duration && IsOurTimeScale())
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // Якщо за цей час хтось інший змінив timeScale (наприклад, пауза) — не перезаписуємо.
            if (IsOurTimeScale())
                Time.timeScale = 1f;

            _isHitStopActive = false;
        }

        private static bool IsOurTimeScale() =>
            Mathf.Approximately(Time.timeScale, HitStopTimeScale);

        private CameraFollow GetCameraFollow()
        {
            if (_cameraFollow != null) return _cameraFollow;

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                mainCamera.TryGetComponent(out _cameraFollow);

            return _cameraFollow;
        }
    }
}
