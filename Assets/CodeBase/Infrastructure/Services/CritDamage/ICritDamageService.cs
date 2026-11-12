using System;

namespace CodeBase.Infrastructure.Services.CritDamage
{
    public interface ICritDamageService : IService
    {
        float GetCritDamage(float damage, int critChance, float critRate);
        event Action<float, bool> OnGetCritDamage;

        /// <summary>Чи був критичним останній прорахований удар (результат останнього GetCritDamage).</summary>
        bool LastHitWasCritical { get; }
    }
}