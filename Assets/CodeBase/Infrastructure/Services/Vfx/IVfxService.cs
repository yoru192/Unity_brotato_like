using CodeBase.StaticData.Vfx;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.Vfx
{
    /// <summary>Програє візуальні ефекти з пулу. Які префаби за якими VfxId — у VfxStaticData.</summary>
    public interface IVfxService : IService
    {
        /// <summary>Разовий ефект у точці. Нічого не робить, якщо для id не призначено префаб.</summary>
        void Play(VfxId id, Vector3 position, float scaleMultiplier = 1f);

        /// <summary>
        /// Ефект, що слідує за об'єктом, доки його не зупинять через Stop (або доки об'єкт не зникне).
        /// Повертає null, якщо для id не призначено префаб.
        /// </summary>
        GameObject PlayFollowing(VfxId id, Transform target);

        /// <summary>Зупиняє ефект, отриманий із PlayFollowing. Безпечно для null і вже зупиненого.</summary>
        void Stop(GameObject instance);
    }
}
