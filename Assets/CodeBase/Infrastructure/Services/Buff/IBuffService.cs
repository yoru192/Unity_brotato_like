using System;

namespace CodeBase.Infrastructure.Services.Buff
{
    public interface IBuffService : IService
    {
        event Action OnWeaponBuffChanged;
        event Action OnHealthBuffChanged;
        event Action OnSpeedBuffChanged;
        void ApplyBuff(BuffType buffType, float value, float duration);
        void StopAllBuffs();
    }
}