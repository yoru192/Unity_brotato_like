using CodeBase.Infrastructure.Services.Perk;
using CodeBase.Logic;
using CodeBase.StaticData.Perk;
using UnityEngine;

namespace CodeBase.Weapon
{
    /// <summary>Модифікатори урону зброї від перків, спільні для всіх видів зброї.</summary>
    public static class PerkDamage
    {
        /// <summary>
        /// Перк Berserk: що менше здоров'я у власника, то більший урон. Значення перку — максимальний бонус
        /// у відсотках, який досягається при майже нульовому здоров'ї; при повному здоров'ї бонусу немає.
        /// </summary>
        public static float WithBerserk(float baseDamage, IPerkService perkService, IHealth owner)
        {
            if (perkService == null || owner == null) return baseDamage;

            float maxBonusPercent = perkService.GetValue(PerkId.Berserk);
            if (maxBonusPercent <= 0f || owner.Max <= 0f) return baseDamage;

            float missingHealth = 1f - Mathf.Clamp01(owner.Current / owner.Max);
            if (missingHealth <= 0f) return baseDamage;

            // Округлюємо, щоб у цифрах урону не з'являлись дроби.
            return Mathf.Round(baseDamage * (1f + maxBonusPercent / 100f * missingHealth));
        }
    }
}
