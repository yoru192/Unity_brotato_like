using System;
using CodeBase.StaticData.Hero;
using CodeBase.StaticData.MetaUpgrade;

namespace CodeBase.Infrastructure.Services.MetaProgress
{
    /// <summary>Прогрес між забігами: постійна валюта (есенція), відкриті герої, дерево апгрейдів, статистика.</summary>
    public interface IMetaProgressService : IService
    {
        /// <summary>Змінилась кількість есенції, список відкритих героїв або рівні апгрейдів.</summary>
        event Action Changed;

        int Essence { get; }
        int RunsFinished { get; }
        int RunsWon { get; }
        int BestNodesCompleted { get; }

        void Load();
        void Save();

        bool IsHeroUnlocked(HeroStaticData hero);
        bool CanUnlockHero(HeroStaticData hero);

        /// <summary>Купує героя за есенцію. Повертає false, якщо не вистачає есенції. Одразу зберігає.</summary>
        bool TryUnlockHero(HeroStaticData hero);

        /// <summary>Скільки рівнів апгрейду куплено. 0, якщо жодного.</summary>
        int GetUpgradeLevel(MetaUpgradeId id);

        /// <summary>Сумарна сила апгрейду: рівень × valuePerLevel. 0, якщо апгрейд не куплено.</summary>
        float GetUpgradeValue(MetaUpgradeId id);

        /// <summary>Скільки рівнів куплено в гілці загалом.</summary>
        int GetBranchLevels(MetaUpgradeBranch branch);

        /// <summary>Чи виконано вимогу requiredBranchLevels (рівні самого апгрейду не рахуються).</summary>
        bool IsUpgradeUnlocked(MetaUpgradeStaticData upgrade);

        /// <summary>Апгрейд відкритий, не на максимумі, і есенції вистачає на наступний рівень.</summary>
        bool CanBuyUpgrade(MetaUpgradeStaticData upgrade);

        /// <summary>Купує наступний рівень апгрейду за есенцію. Одразу зберігає.</summary>
        bool TryBuyUpgrade(MetaUpgradeStaticData upgrade);

        /// <summary>
        /// Старт забігу: записує у свіжий PlayerProgress базові характеристики героя та зброї разом із бонусами дерева,
        /// видає стартове золото й перки. Викликати один раз, щойно гравець підтвердив героя.
        /// </summary>
        void ApplyRunBonuses(HeroStaticData hero);

        /// <summary>
        /// Фіксує кінець забігу: нараховує есенцію за пройдені вузли карти (і бонус за перемогу),
        /// оновлює статистику та зберігає. Повертає підсумок забігу (вузли, есенція, рекорд).
        /// </summary>
        RunSummary CompleteRun(bool won);
    }
}
