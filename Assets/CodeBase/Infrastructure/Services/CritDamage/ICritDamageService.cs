using System;

namespace CodeBase.Infrastructure.Services.CritDamage
{
    public interface ICritDamageService : IService
    {
        float GetCritDamage(float damage, int critChance, float critRate);
        event Action<float, bool> OnGetCritDamage;
    }
}