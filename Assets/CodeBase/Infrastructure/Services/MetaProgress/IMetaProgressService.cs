using System;
using CodeBase.StaticData.Hero;

namespace CodeBase.Infrastructure.Services.MetaProgress
{
    /// <summary>Прогрес між забігами: постійна валюта (есенція), відкриті герої, статистика.</summary>
    public interface IMetaProgressService : IService
    {
        /// <summary>Змінилась кількість есенції або список відкритих героїв.</summary>
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

        /// <summary>
        /// Фіксує кінець забігу: нараховує есенцію за пройдені вузли карти (і бонус за перемогу),
        /// оновлює статистику та зберігає. Повертає нараховану есенцію.
        /// </summary>
        int CompleteRun(bool won);
    }
}
