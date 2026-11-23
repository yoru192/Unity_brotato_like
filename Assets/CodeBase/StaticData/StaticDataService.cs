using System.Collections.Generic;
using System.Linq;
using CodeBase.StaticData.Ability;
using CodeBase.StaticData.Enemy;
using CodeBase.StaticData.Hero;
using CodeBase.StaticData.Map;
using CodeBase.StaticData.Weapon;
using UnityEngine;

namespace CodeBase.StaticData
{
    public class StaticDataService : IStaticDataService
    {
        private Dictionary<string, LevelStaticData> _levels;
        private Dictionary<WeaponTypeId, WeaponStaticData> _weapons;
        private Dictionary<EnemyTypeId, EnemyStaticData> _enemies;
        private Dictionary<EnemyTypeId, BossStaticData> _bosses;
        private Dictionary<AbilityTypeId, AbilityStaticData> _abilities;
        private Dictionary<HeroTypeId, HeroStaticData> _heroes;
        private List<UpgradeStaticData> _upgrades;
        private List<ShopItemStaticData> _shopItems;
        private WaveControllerStaticData _waveController;
        private MapStaticData _mapSettings;
        private MapLevelLibrary _mapLevelLibrary;
        private AudioStaticData _audio;
        private MusicStaticData _music;
        private Vfx.VfxStaticData _vfx;
        private List<MetaUpgrade.MetaUpgradeStaticData> _metaUpgradeList;
        private Dictionary<MetaUpgrade.MetaUpgradeId, MetaUpgrade.MetaUpgradeStaticData> _metaUpgrades;


        public void Load()
        {
            _levels = Resources.LoadAll<LevelStaticData>("StaticData/Levels")
                .ToDictionary(x => x.levelKey,  x => x);
            _weapons = Resources.LoadAll<WeaponStaticData>("StaticData/Weapons")
                .ToDictionary(x => x.weaponTypeId,  x => x);
            _enemies = Resources.LoadAll<EnemyStaticData>("StaticData/Enemy")
                .ToDictionary(x => x.enemyTypeId,  x => x);
            _bosses = Resources.LoadAll<BossStaticData>("StaticData/Bosses")
                .ToDictionary(x => x.enemyTypeId,  x => x);
            _abilities = Resources.LoadAll<AbilityStaticData>("StaticData/Abilities")
                .ToDictionary(x => x.abilityTypeId, x => x);
            _heroes = Resources.LoadAll<HeroStaticData>("StaticData/Heroes")
                .ToDictionary(x => x.heroTypeId,  x => x);
            _upgrades = Resources.LoadAll<UpgradeStaticData>("StaticData/Upgrades")
                .ToList();
            _shopItems = Resources.LoadAll<ShopItemStaticData>("StaticData/ShopItems")
                .ToList();
            _waveController = Resources.Load<WaveControllerStaticData>("StaticData/Waves/WaveController");
            _mapSettings = Resources.Load<MapStaticData>("StaticData/Map/MapStaticData");
            _mapLevelLibrary = Resources.Load<MapLevelLibrary>("StaticData/Map/MapLevelLibrary");
            _audio = Resources.Load<AudioStaticData>("StaticData/Audio/AudioStaticData");
            _music = Resources.Load<MusicStaticData>("StaticData/Audio/MusicStaticData");
            _vfx = Resources.Load<Vfx.VfxStaticData>("StaticData/Vfx/VfxStaticData");
            _metaUpgradeList = Resources.LoadAll<MetaUpgrade.MetaUpgradeStaticData>("StaticData/MetaUpgrades")
                .OrderBy(x => x.branch)
                .ThenBy(x => x.order)
                .ToList();
            _metaUpgrades = _metaUpgradeList.ToDictionary(x => x.id, x => x);
        }

        public Vfx.VfxStaticData GetVfx() => _vfx;

        public List<MetaUpgrade.MetaUpgradeStaticData> GetAllMetaUpgrades() => _metaUpgradeList;

        public MetaUpgrade.MetaUpgradeStaticData ForMetaUpgrade(MetaUpgrade.MetaUpgradeId id) =>
            _metaUpgrades.TryGetValue(id, out MetaUpgrade.MetaUpgradeStaticData staticData)
                ? staticData
                : null;
        
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
        public BossStaticData ForBoss(EnemyTypeId enemyId) =>
            _bosses.TryGetValue(enemyId, out BossStaticData staticData)
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

        public MapStaticData GetMapSettings() => _mapSettings;
        public MapLevelLibrary GetMapLevelLibrary() => _mapLevelLibrary;
        public AudioStaticData GetAudio() => _audio;
        public MusicStaticData GetMusic() => _music;
        
    }
}