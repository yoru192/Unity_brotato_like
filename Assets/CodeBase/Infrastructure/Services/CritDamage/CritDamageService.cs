using System;
using Random = UnityEngine.Random;

namespace CodeBase.Infrastructure.Services.CritDamage
{
    public class CritDamageService : ICritDamageService
    {
        public event Action<float, bool> OnGetCritDamage;

        public float GetCritDamage(float damage, int critChance, float critRate)
        {
            int randomValue = Random.Range(0, 100);
            bool isCritical = randomValue >= critChance;

            if (isCritical)
            {
                damage *= critRate;
                damage = MathF.Round(damage, MidpointRounding.AwayFromZero);
            }

            OnGetCritDamage?.Invoke(damage, isCritical);
            return damage;
        }
    }
}