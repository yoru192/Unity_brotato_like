using System;
using Random = UnityEngine.Random;

namespace CodeBase.Infrastructure.Services.CritDamage
{
    public class CritDamageService : ICritDamageService
    {
        public event Action<float, bool> OnGetCritDamage;

        public bool LastHitWasCritical { get; private set; }

        public float GetCritDamage(float damage, int critChance, float critRate)
        {
            int randomValue = Random.Range(0, 100);
            // randomValue рівномірно в 0..99, тож critChance — це відсоток: 0 = ніколи, 100 = завжди.
            bool isCritical = randomValue < critChance;

            if (isCritical)
            {
                damage *= critRate;
                damage = MathF.Round(damage, MidpointRounding.AwayFromZero);
            }

            LastHitWasCritical = isCritical;
            OnGetCritDamage?.Invoke(damage, isCritical);
            return damage;
        }
    }
}