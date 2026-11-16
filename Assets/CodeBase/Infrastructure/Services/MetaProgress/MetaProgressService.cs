using System;
using System.Collections.Generic;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.Perk;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.StaticData;
using CodeBase.StaticData.Hero;
using CodeBase.StaticData.Map;
using CodeBase.StaticData.MetaUpgrade;
using CodeBase.StaticData.Perk;
using CodeBase.StaticData.Weapon;
using UnityEngine;
using MetaProgressData = CodeBase.Data.MetaProgress;

namespace CodeBase.Infrastructure.Services.MetaProgress
{
    public class MetaProgressService : IMetaProgressService
    {
        private const string PrefsKey = "MetaProgress";

        // Запобіжник: апгрейди перезарядки не можуть зробити зброю миттєвою.
        private const float MinWeaponCooldown = 0.1f;

        private readonly IStaticDataService _staticData;
        private readonly IPersistentProgressService _persistentProgress;
        private readonly IPerkService _perkService;

        private MetaProgressData _data = new MetaProgressData();

        public event Action Changed;

        public MetaProgressService(IStaticDataService staticData, IPersistentProgressService persistentProgress,
            IPerkService perkService)
        {
            _staticData = staticData;
            _persistentProgress = persistentProgress;
            _perkService = perkService;
        }

        public int Essence => _data.essence;
        public int RunsFinished => _data.runsFinished;
        public int RunsWon => _data.runsWon;
        public int BestNodesCompleted => _data.bestNodesCompleted;

        public void Load()
        {
            string json = PlayerPrefs.GetString(PrefsKey, string.Empty);

            MetaProgressData loaded = null;
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    loaded = JsonUtility.FromJson<MetaProgressData>(json);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Не вдалося прочитати MetaProgress, починаємо з чистого: {e.Message}");
                }
            }

            _data = loaded ?? new MetaProgressData();
            if (_data.unlockedHeroes == null)
                _data.unlockedHeroes = new List<HeroTypeId>();
            // Збереження, зроблені до появи дерева апгрейдів, цього списку не містять.
            if (_data.upgrades == null)
                _data.upgrades = new List<MetaUpgradeEntry>();
        }

        public void Save()
        {
            PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(_data));
            PlayerPrefs.Save();
        }

        public bool IsHeroUnlocked(HeroStaticData hero)
        {
            if (hero == null) return false;
            return hero.unlockCost <= 0 || _data.unlockedHeroes.Contains(hero.heroTypeId);
        }

        public bool CanUnlockHero(HeroStaticData hero) =>
            hero != null && !IsHeroUnlocked(hero) && _data.essence >= hero.unlockCost;

        public bool TryUnlockHero(HeroStaticData hero)
        {
            if (hero == null) return false;
            if (IsHeroUnlocked(hero)) return true;
            if (_data.essence < hero.unlockCost) return false;

            _data.essence -= hero.unlockCost;
            _data.unlockedHeroes.Add(hero.heroTypeId);

            Save();
            Changed?.Invoke();
            return true;
        }

        public int GetUpgradeLevel(MetaUpgradeId id)
        {
            int index = IndexOfUpgrade(id);
            return index >= 0 ? _data.upgrades[index].level : 0;
        }

        public float GetUpgradeValue(MetaUpgradeId id)
        {
            int level = GetUpgradeLevel(id);
            if (level <= 0) return 0f;

            MetaUpgradeStaticData upgrade = _staticData.ForMetaUpgrade(id);
            return upgrade != null ? level * upgrade.valuePerLevel : 0f;
        }

        public int GetBranchLevels(MetaUpgradeBranch branch)
        {
            int total = 0;
            List<MetaUpgradeStaticData> all = _staticData.GetAllMetaUpgrades();
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].branch == branch)
                    total += GetUpgradeLevel(all[i].id);
            }

            return total;
        }

        public bool IsUpgradeUnlocked(MetaUpgradeStaticData upgrade)
        {
            if (upgrade == null) return false;
            if (upgrade.requiredBranchLevels <= 0) return true;

            int otherLevels = GetBranchLevels(upgrade.branch) - GetUpgradeLevel(upgrade.id);
            return otherLevels >= upgrade.requiredBranchLevels;
        }

        public bool CanBuyUpgrade(MetaUpgradeStaticData upgrade)
        {
            if (upgrade == null || !IsUpgradeUnlocked(upgrade)) return false;

            int cost = upgrade.CostForNextLevel(GetUpgradeLevel(upgrade.id));
            return cost >= 0 && _data.essence >= cost;
        }

        public bool TryBuyUpgrade(MetaUpgradeStaticData upgrade)
        {
            if (!CanBuyUpgrade(upgrade)) return false;

            int level = GetUpgradeLevel(upgrade.id);
            _data.essence -= upgrade.CostForNextLevel(level);
            SetUpgradeLevel(upgrade.id, level + 1);

            Save();
            Changed?.Invoke();
            return true;
        }

        public void ApplyRunBonuses(HeroStaticData hero)
        {
            PlayerProgress progress = _persistentProgress.Progress;
            if (hero == null || progress == null) return;

            // Характеристики тут присвоюються, а не додаються: компоненти (PlayerHealth, PlayerMovement, зброя)
            // підставляють базові значення лише коли в прогресі стоїть 0, тож готові числа вони не чіпають.
            State state = progress.playerState;

            state.maxHealth = hero.maxHealth * (1f + GetUpgradeValue(MetaUpgradeId.Toughness) / 100f);
            state.currentHealth = state.maxHealth;
            state.moveSpeed = hero.moveSpeed + GetUpgradeValue(MetaUpgradeId.LightFeet);

            // Обидві зброї одразу, навіть якщо героя озброєно лише однією:
            // так зброя, куплена пізніше в магазині, теж отримає бонуси дерева.
            WeaponStaticData melee = _staticData.ForWeapon(WeaponTypeId.Melee);
            if (melee != null)
                SeedWeapon(state.MeleeWeaponState, melee.damage, melee.cooldown);

            WeaponStaticData arrow = _staticData.ForWeapon(WeaponTypeId.Arrow);
            if (arrow != null)
                SeedWeapon(state.ArrowWeaponState, arrow.damage, arrow.shootRate);

            state.currentBalance = Mathf.RoundToInt(GetUpgradeValue(MetaUpgradeId.StartingGold));

            float secondChance = GetUpgradeValue(MetaUpgradeId.SecondChance);
            if (secondChance > 0f && !_perkService.Has(PerkId.SecondChance))
                _perkService.Add(PerkId.SecondChance, secondChance);
        }

        public int CompleteRun(bool won)
        {
            int nodesCompleted = NodesCompletedThisRun();
            int reward = CalculateReward(nodesCompleted, won);

            _data.essence += reward;
            _data.runsFinished++;
            if (won)
                _data.runsWon++;
            if (nodesCompleted > _data.bestNodesCompleted)
                _data.bestNodesCompleted = nodesCompleted;

            Save();
            Changed?.Invoke();
            return reward;
        }

        private int NodesCompletedThisRun()
        {
            PlayerProgress progress = _persistentProgress.Progress;
            if (progress == null || progress.mapProgress == null || progress.mapProgress.completedNodeIds == null)
                return 0;

            return progress.mapProgress.completedNodeIds.Count;
        }

        private int CalculateReward(int nodesCompleted, bool won)
        {
            MapStaticData settings = _staticData.GetMapSettings();
            if (settings == null) return 0;

            int reward = nodesCompleted * settings.essencePerCompletedNode;
            if (won)
                reward += settings.essenceForWin;

            float multiplier = 1f + GetUpgradeValue(MetaUpgradeId.EssenceGreed) / 100f;
            return Mathf.Max(0, Mathf.RoundToInt(reward * multiplier));
        }

        private void SeedWeapon(WeaponState weapon, float baseDamage, float baseCooldown)
        {
            weapon.weaponDamage = baseDamage + GetUpgradeValue(MetaUpgradeId.SharpBlade);
            weapon.weaponCooldown = Mathf.Max(
                Mathf.Min(MinWeaponCooldown, baseCooldown),
                baseCooldown - GetUpgradeValue(MetaUpgradeId.QuickHands));
        }

        private void SetUpgradeLevel(MetaUpgradeId id, int level)
        {
            MetaUpgradeEntry entry = new MetaUpgradeEntry { id = id, level = level };

            int index = IndexOfUpgrade(id);
            if (index >= 0)
                _data.upgrades[index] = entry;
            else
                _data.upgrades.Add(entry);
        }

        private int IndexOfUpgrade(MetaUpgradeId id)
        {
            for (int i = 0; i < _data.upgrades.Count; i++)
            {
                if (_data.upgrades[i].id == id)
                    return i;
            }

            return -1;
        }
    }
}
