using System.Collections.Generic;
using System.Linq;
using CodeBase.StaticData.Enemy;
using CodeBase.StaticData.Hero;
using CodeBase.StaticData.Weapon;
using UnityEngine;

namespace CodeBase.StaticData
{
    public class StaticDataService : IStaticDataService
    {
        private Dictionary<string, LevelStaticData> _levels;
        private Dictionary<WeaponTypeId, WeaponStaticData> _weapons;
        private Dictionary<EnemyTypeId, EnemyStaticData> _enemies;
        private Dictionary<AbilityTypeId, AbilityStaticData> _abilities;
        private Dictionary<HeroTypeId, HeroStaticData> _heroes;
        private List<UpgradeStaticData> _upgrades;
        private List<ShopItemStaticData> _shopItems;
        private WaveControllerStaticData _waveController;


        public void Load()
        {
            _levels = Resources.LoadAll<LevelStaticData>("StaticData/Levels")
                .ToDictionary(x => x.levelKey,  x => x);
            _weapons = Resources.LoadAll<WeaponStaticData>("StaticData/Weapons")
                .ToDictionary(x => x.weaponTypeId,  x => x);
            _enemies = Resources.LoadAll<EnemyStaticData>("StaticData/Enemy")
                .ToDictionary(x => x.enemyTypeId,  x => x);
            _abilities = Resources.LoadAll<AbilityStaticData>("StaticData/Abilities")
                .ToDictionary(x => x.abilityTypeId, x => x);
            _heroes = Resources.LoadAll<HeroStaticData>("StaticData/Heroes")
                .ToDictionary(x => x.heroTypeId,  x => x);
            _upgrades = Resources.LoadAll<UpgradeStaticData>("StaticData/Upgrades")
                .ToList();
            _shopItems = Resources.LoadAll<ShopItemStaticData>("StaticData/ShopItems")
                .ToList();
            _waveController = Resources.Load<WaveControllerStaticData>("StaticData/WaveController");
        }
        
        public LevelStaticData ForLevel(string sceneKey) =>
            _levels.TryGetValue(sceneKey, out LevelStaticData staticData)
                ? staticData
                : null;
        public WeaponStaticData ForWeapon(WeaponTypeId weaponId) =>
            _weapons.TryGetValue(weaponId, out WeaponStaticData staticData)
                ? staticData
                : null;
        public EnemyStaticData ForEnemy(EnemyTypeId enemyId) =>
            _enemies.TryGetValue(enemyId, out EnemyStaticData staticData)
                ? staticData
                : null;

        public AbilityStaticData ForAbility(AbilityTypeId abilityId) =>
            _abilities.TryGetValue(abilityId, out AbilityStaticData staticData)
                ? staticData
                : null;
        public HeroStaticData ForHero(HeroTypeId heroId) =>
            _heroes.TryGetValue(heroId, out HeroStaticData staticData)
                ? staticData
                : null;
        public List<UpgradeStaticData> GetAllUpgrades() => _upgrades;
        public List<ShopItemStaticData> GetAllShopItems() => _shopItems;
        
        public WaveControllerStaticData GetWaveController() => _waveController;
    }
}