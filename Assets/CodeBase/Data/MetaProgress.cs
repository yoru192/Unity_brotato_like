using System;
using System.Collections.Generic;
using CodeBase.StaticData.Hero;
using CodeBase.StaticData.MetaUpgrade;

namespace CodeBase.Data
{
    /// <summary>
    /// Прогрес між забігами. На відміну від <see cref="PlayerProgress"/>, не скидається на старті нового забігу
    /// і зберігається на диск (PlayerPrefs).
    /// </summary>
    [Serializable]
    public class MetaProgress
    {
        /// <summary>Постійна валюта, яку гравець отримує наприкінці кожного забігу.</summary>
        public int essence;

        /// <summary>Герої, куплені за есенцію. Герої з unlockCost = 0 доступні завжди і сюди не потрапляють.</summary>
        public List<HeroTypeId> unlockedHeroes = new List<HeroTypeId>();

        /// <summary>Куплені рівні дерева мета-апгрейдів. Працювати з ними через IMetaProgressService.</summary>
        public List<MetaUpgradeEntry> upgrades = new List<MetaUpgradeEntry>();

        public int runsFinished;
        public int runsWon;
        public int bestNodesCompleted;
    }

    [Serializable]
    public struct MetaUpgradeEntry
    {
        public MetaUpgradeId id;
        public int level;
    }
}
