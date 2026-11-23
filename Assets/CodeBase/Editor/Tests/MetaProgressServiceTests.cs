using System.Collections.Generic;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.MetaProgress;
using CodeBase.Infrastructure.Services.Perk;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.StaticData.Hero;
using CodeBase.StaticData.Map;
using CodeBase.StaticData.MetaUpgrade;
using CodeBase.StaticData.Perk;
using CodeBase.StaticData.Weapon;
using NUnit.Framework;
using UnityEngine;
using MetaProgressData = CodeBase.Data.MetaProgress;

namespace CodeBase.Editor.Tests
{
    /// <summary>
    /// EditMode-тести дерева мета-апгрейдів. MetaProgressService пише в PlayerPrefs, тому справжнє збереження
    /// гравця відкладається в SetUp і повертається в TearDown.
    /// </summary>
    public class MetaProgressServiceTests
    {
        private const string PrefsKey = "MetaProgress";

        private static readonly int[] FiveLevelCosts = { 10, 20, 30, 45, 60 };
        private static readonly int[] ThreeLevelCosts = { 25, 45, 70 };
        private static readonly int[] CapstoneCost = { 200 };

        private readonly List<Object> _created = new List<Object>();

        private bool _hadSavedPrefs;
        private string _savedPrefs;

        private FakeStaticDataService _staticData;
        private PersistentProgressService _persistentProgress;
        private PerkService _perkService;
        private HeroStaticData _hero;

        private MetaUpgradeStaticData _toughness;
        private MetaUpgradeStaticData _campfire;
        private MetaUpgradeStaticData _secondChance;
        private MetaUpgradeStaticData _quickHands;

        [SetUp]
        public void SetUp()
        {
            _hadSavedPrefs = PlayerPrefs.HasKey(PrefsKey);
            _savedPrefs = PlayerPrefs.GetString(PrefsKey, string.Empty);
            PlayerPrefs.DeleteKey(PrefsKey);

            _staticData = new FakeStaticDataService();
            _staticData.MapSettings = Create<MapStaticData>();
            _staticData.MapSettings.essencePerCompletedNode = 5;
            _staticData.MapSettings.essenceForWin = 50;

            WeaponStaticData melee = Create<WeaponStaticData>();
            melee.weaponTypeId = WeaponTypeId.Melee;
            melee.damage = 3;
            melee.cooldown = 0.8f;
            _staticData.Weapons[WeaponTypeId.Melee] = melee;

            WeaponStaticData arrow = Create<WeaponStaticData>();
            arrow.weaponTypeId = WeaponTypeId.Arrow;
            arrow.damage = 2;
            arrow.shootRate = 1.5f;
            _staticData.Weapons[WeaponTypeId.Arrow] = arrow;

            _toughness = AddUpgrade(MetaUpgradeId.Toughness, MetaUpgradeBranch.Survival, 4f, FiveLevelCosts, 0);
            _campfire = AddUpgrade(MetaUpgradeId.CampfireWarmth, MetaUpgradeBranch.Survival, 5f, ThreeLevelCosts, 0);
            _secondChance = AddUpgrade(MetaUpgradeId.SecondChance, MetaUpgradeBranch.Survival, 30f, CapstoneCost, 5);
            _quickHands = AddUpgrade(MetaUpgradeId.QuickHands, MetaUpgradeBranch.Combat, 0.02f, FiveLevelCosts, 0);
            AddUpgrade(MetaUpgradeId.LightFeet, MetaUpgradeBranch.Combat, 0.2f, ThreeLevelCosts, 0);
            AddUpgrade(MetaUpgradeId.SharpBlade, MetaUpgradeBranch.Combat, 1f, CapstoneCost, 0);
            AddUpgrade(MetaUpgradeId.StartingGold, MetaUpgradeBranch.Cunning, 10f, FiveLevelCosts, 0);
            AddUpgrade(MetaUpgradeId.EssenceGreed, MetaUpgradeBranch.Cunning, 5f, ThreeLevelCosts, 0);

            _hero = Create<HeroStaticData>();
            _hero.maxHealth = 100f;
            _hero.moveSpeed = 7f;

            _persistentProgress = new PersistentProgressService { Progress = new PlayerProgress("Main") };
            _perkService = new PerkService(_persistentProgress);
        }

        [TearDown]
        public void TearDown()
        {
            if (_hadSavedPrefs)
                PlayerPrefs.SetString(PrefsKey, _savedPrefs);
            else
                PlayerPrefs.DeleteKey(PrefsKey);
            PlayerPrefs.Save();

            foreach (Object created in _created)
                Object.DestroyImmediate(created);
            _created.Clear();
        }

        [Test]
        public void CompleteRun_Won_GivesEssenceForNodesAndWin()
        {
            MetaProgressService service = CreateService(essence: 0);
            CompleteNodes(3);

            RunSummary summary = service.CompleteRun(won: true);

            Assert.AreEqual(3 * 5 + 50, summary.EssenceReward);
            Assert.AreEqual(summary.EssenceReward, service.Essence);
            Assert.AreEqual(service.Essence, summary.EssenceTotal);
            Assert.AreEqual(3, summary.NodesCompleted);
            Assert.AreEqual(1, service.RunsWon);
        }

        [Test]
        public void CompleteRun_FlagsNewBestOnlyWhenNodesRecordIsBeaten()
        {
            MetaProgressService service = CreateService(essence: 0);
            CompleteNodes(3);

            Assert.IsTrue(service.CompleteRun(won: false).IsNewBest);
            Assert.IsFalse(service.CompleteRun(won: false).IsNewBest);
        }

        [Test]
        public void CompleteRun_WithEssenceGreed_MultipliesReward()
        {
            MetaProgressService service = CreateService(essence: 1000);
            MetaUpgradeStaticData greed = _staticData.ForMetaUpgrade(MetaUpgradeId.EssenceGreed);
            service.TryBuyUpgrade(greed);
            service.TryBuyUpgrade(greed);
            CompleteNodes(10);

            int reward = service.CompleteRun(won: true).EssenceReward;

            // (10 * 5 + 50) * 1.10
            Assert.AreEqual(110, reward);
        }

        [Test]
        public void TryBuyUpgrade_SpendsEssenceAndRaisesLevel()
        {
            MetaProgressService service = CreateService(essence: 35);

            Assert.IsTrue(service.TryBuyUpgrade(_toughness));
            Assert.IsTrue(service.TryBuyUpgrade(_toughness));

            Assert.AreEqual(2, service.GetUpgradeLevel(MetaUpgradeId.Toughness));
            Assert.AreEqual(8f, service.GetUpgradeValue(MetaUpgradeId.Toughness), 0.0001f);
            Assert.AreEqual(35 - 10 - 20, service.Essence);
        }

        [Test]
        public void TryBuyUpgrade_NotEnoughEssence_ChangesNothing()
        {
            MetaProgressService service = CreateService(essence: 9);

            Assert.IsFalse(service.CanBuyUpgrade(_toughness));
            Assert.IsFalse(service.TryBuyUpgrade(_toughness));

            Assert.AreEqual(0, service.GetUpgradeLevel(MetaUpgradeId.Toughness));
            Assert.AreEqual(9, service.Essence);
        }

        [Test]
        public void TryBuyUpgrade_AtMaxLevel_Fails()
        {
            MetaProgressService service = CreateService(essence: 1000);
            for (int i = 0; i < _campfire.MaxLevel; i++)
                Assert.IsTrue(service.TryBuyUpgrade(_campfire));

            int essenceBefore = service.Essence;

            Assert.IsFalse(service.TryBuyUpgrade(_campfire));
            Assert.AreEqual(_campfire.MaxLevel, service.GetUpgradeLevel(MetaUpgradeId.CampfireWarmth));
            Assert.AreEqual(essenceBefore, service.Essence);
        }

        [Test]
        public void Capstone_IsLockedUntilBranchHasRequiredLevels()
        {
            MetaProgressService service = CreateService(essence: 1000);

            for (int i = 0; i < 4; i++)
                service.TryBuyUpgrade(_toughness);

            Assert.IsFalse(service.IsUpgradeUnlocked(_secondChance));
            Assert.IsFalse(service.TryBuyUpgrade(_secondChance));

            service.TryBuyUpgrade(_campfire);

            Assert.AreEqual(5, service.GetBranchLevels(MetaUpgradeBranch.Survival));
            Assert.IsTrue(service.IsUpgradeUnlocked(_secondChance));
            Assert.IsTrue(service.TryBuyUpgrade(_secondChance));
        }

        [Test]
        public void Capstone_IgnoresLevelsFromOtherBranches()
        {
            MetaProgressService service = CreateService(essence: 1000);

            for (int i = 0; i < 5; i++)
                service.TryBuyUpgrade(_quickHands);

            Assert.IsFalse(service.IsUpgradeUnlocked(_secondChance));
        }

        [Test]
        public void Load_RestoresBoughtLevelsFromPlayerPrefs()
        {
            MetaProgressService service = CreateService(essence: 100);
            service.TryBuyUpgrade(_toughness);
            service.TryBuyUpgrade(_quickHands);

            MetaProgressService reloaded = new MetaProgressService(_staticData, _persistentProgress, _perkService);
            reloaded.Load();

            Assert.AreEqual(1, reloaded.GetUpgradeLevel(MetaUpgradeId.Toughness));
            Assert.AreEqual(1, reloaded.GetUpgradeLevel(MetaUpgradeId.QuickHands));
            Assert.AreEqual(100 - 10 - 10, reloaded.Essence);
        }

        [Test]
        public void Load_SaveWithoutUpgradesField_StartsWithEmptyTree()
        {
            // Збереження з версії до появи дерева: поля upgrades у JSON немає.
            PlayerPrefs.SetString(PrefsKey, "{\"essence\":40,\"unlockedHeroes\":[],\"runsFinished\":2}");
            MetaProgressService service = new MetaProgressService(_staticData, _persistentProgress, _perkService);

            service.Load();

            Assert.AreEqual(40, service.Essence);
            Assert.AreEqual(0, service.GetUpgradeLevel(MetaUpgradeId.Toughness));
            Assert.IsTrue(service.TryBuyUpgrade(_toughness));
        }

        [Test]
        public void ApplyRunBonuses_WithoutUpgrades_SeedsBaseStats()
        {
            MetaProgressService service = CreateService(essence: 0);

            service.ApplyRunBonuses(_hero);

            State state = _persistentProgress.Progress.playerState;
            Assert.AreEqual(100f, state.maxHealth, 0.001f);
            Assert.AreEqual(100f, state.currentHealth, 0.001f);
            Assert.AreEqual(7f, state.moveSpeed, 0.001f);
            Assert.AreEqual(3f, state.MeleeWeaponState.weaponDamage, 0.001f);
            Assert.AreEqual(0.8f, state.MeleeWeaponState.weaponCooldown, 0.001f);
            Assert.AreEqual(2f, state.ArrowWeaponState.weaponDamage, 0.001f);
            Assert.AreEqual(1.5f, state.ArrowWeaponState.weaponCooldown, 0.001f);
            Assert.AreEqual(0, state.currentBalance);
            Assert.IsFalse(_perkService.Has(PerkId.SecondChance));
        }

        [Test]
        public void ApplyRunBonuses_AppliesStatUpgradesToHeroAndBothWeapons()
        {
            MetaProgressService service = CreateService(essence: 5000);
            BuyLevels(service, MetaUpgradeId.Toughness, 5);
            BuyLevels(service, MetaUpgradeId.QuickHands, 5);
            BuyLevels(service, MetaUpgradeId.LightFeet, 3);
            BuyLevels(service, MetaUpgradeId.SharpBlade, 1);
            BuyLevels(service, MetaUpgradeId.StartingGold, 5);

            service.ApplyRunBonuses(_hero);

            State state = _persistentProgress.Progress.playerState;
            Assert.AreEqual(120f, state.maxHealth, 0.001f);
            Assert.AreEqual(120f, state.currentHealth, 0.001f);
            Assert.AreEqual(7.6f, state.moveSpeed, 0.001f);
            Assert.AreEqual(4f, state.MeleeWeaponState.weaponDamage, 0.001f);
            Assert.AreEqual(0.7f, state.MeleeWeaponState.weaponCooldown, 0.001f);
            Assert.AreEqual(3f, state.ArrowWeaponState.weaponDamage, 0.001f);
            Assert.AreEqual(1.4f, state.ArrowWeaponState.weaponCooldown, 0.001f);
            Assert.AreEqual(50, state.currentBalance);
        }

        [Test]
        public void ApplyRunBonuses_WithSecondChance_GrantsPerkOnce()
        {
            MetaProgressService service = CreateService(essence: 5000);
            BuyLevels(service, MetaUpgradeId.Toughness, 5);
            BuyLevels(service, MetaUpgradeId.SecondChance, 1);

            service.ApplyRunBonuses(_hero);
            service.ApplyRunBonuses(_hero);

            Assert.AreEqual(30f, _perkService.GetValue(PerkId.SecondChance), 0.001f);
            Assert.AreEqual(1, _perkService.GetPicks(PerkId.SecondChance));
        }

        [Test]
        public void ApplyRunBonuses_CooldownNeverDropsBelowFloor()
        {
            _quickHands.valuePerLevel = 1f;
            MetaProgressService service = CreateService(essence: 5000);
            BuyLevels(service, MetaUpgradeId.QuickHands, 5);

            service.ApplyRunBonuses(_hero);

            State state = _persistentProgress.Progress.playerState;
            Assert.AreEqual(0.1f, state.MeleeWeaponState.weaponCooldown, 0.001f);
            Assert.AreEqual(0.1f, state.ArrowWeaponState.weaponCooldown, 0.001f);
        }

        private MetaProgressService CreateService(int essence)
        {
            PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(new MetaProgressData { essence = essence }));

            MetaProgressService service = new MetaProgressService(_staticData, _persistentProgress, _perkService);
            service.Load();
            return service;
        }

        private void BuyLevels(MetaProgressService service, MetaUpgradeId id, int levels)
        {
            MetaUpgradeStaticData upgrade = _staticData.ForMetaUpgrade(id);
            for (int i = 0; i < levels; i++)
                Assert.IsTrue(service.TryBuyUpgrade(upgrade), $"Could not buy level {i + 1} of {id}");
        }

        private void CompleteNodes(int count)
        {
            List<int> completed = _persistentProgress.Progress.mapProgress.completedNodeIds;
            for (int i = 0; i < count; i++)
                completed.Add(i);
        }

        private MetaUpgradeStaticData AddUpgrade(MetaUpgradeId id, MetaUpgradeBranch branch, float valuePerLevel,
            int[] costs, int requiredBranchLevels)
        {
            MetaUpgradeStaticData upgrade = Create<MetaUpgradeStaticData>();
            upgrade.id = id;
            upgrade.branch = branch;
            upgrade.valuePerLevel = valuePerLevel;
            upgrade.costs = costs;
            upgrade.requiredBranchLevels = requiredBranchLevels;
            _staticData.MetaUpgrades.Add(upgrade);
            return upgrade;
        }

        private T Create<T>() where T : ScriptableObject
        {
            T instance = ScriptableObject.CreateInstance<T>();
            _created.Add(instance);
            return instance;
        }
    }
}
