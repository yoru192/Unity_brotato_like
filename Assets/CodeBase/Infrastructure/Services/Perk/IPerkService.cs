using System;
using CodeBase.StaticData.Perk;

namespace CodeBase.Infrastructure.Services.Perk
{
    /// <summary>
    /// Перки поточного забігу. Зберігаються в PlayerProgress.playerState, тож скидаються разом із забігом.
    /// </summary>
    public interface IPerkService : IService
    {
        /// <summary>Гравець отримав перк або його стак.</summary>
        event Action Changed;

        /// <summary>Сумарне значення перку (сума value всіх узятих апгрейдів цього перку). 0, якщо перку немає.</summary>
        float GetValue(PerkId id);

        /// <summary>Скільки разів гравець брав цей перк.</summary>
        int GetPicks(PerkId id);

        bool Has(PerkId id);

        void Add(PerkId id, float value);

        /// <summary>Повністю забирає перк (для одноразових, що витрачаються при спрацюванні).</summary>
        void Remove(PerkId id);
    }
}
