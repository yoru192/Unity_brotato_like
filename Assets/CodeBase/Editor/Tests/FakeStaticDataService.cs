using System.Collections.Generic;
using CodeBase.StaticData;
using CodeBase.StaticData.Ability;
using CodeBase.StaticData.Enemy;
using CodeBase.StaticData.Hero;
using CodeBase.StaticData.Map;
using CodeBase.StaticData.MetaUpgrade;
using CodeBase.StaticData.Vfx;
using CodeBase.StaticData.Weapon;

namespace CodeBase.Editor.Tests
{
    /// <summary>
    /// Static data для тестів: віддає лише те, що тест поклав у публічні поля, і нічого не читає з Resources.
    /// </summary>
    public class FakeStaticDataService : IStaticDataService
    {
        public readonly Dictionary<WeaponTypeId, WeaponStaticData> Weapons = new Dictionary<WeaponTypeId, WeaponStaticData>();
        public readonly List<MetaUpgradeStaticData> MetaUpgrades = new List<MetaUpgradeStaticData>();
        public MapStaticData MapSettings;

        public void Load()
        {
        }

        public WeaponStaticData ForWeapon(WeaponTypeId weaponId) =>
            Weapons.TryGetValue(weaponId, out WeaponStaticData data) ? data : null;

        public MapStaticData GetMapSettings() => MapSettings;

        public List<MetaUpgradeStaticData> GetAllMetaUpgrades() => MetaUpgrades;

        public MetaUpgradeStaticData ForMetaUpgrade(MetaUpgradeId id)
        {
            foreach (MetaUpgradeStaticData upgrade in MetaUpgrades)
            {
                if (upgrade.id == id)
                    return upgrade;
            }

            return null;
        }

        public LevelStaticData ForLevel(string sceneKey) => null;
        public EnemyStaticData ForEnemy(EnemyTypeId enemyId) => null;
        public List<UpgradeStaticData> GetAllUpgrades() => new List<UpgradeStaticData>();
        public AbilityStaticData ForAbility(AbilityTypeId abilityId) => null;
        public WaveControllerStaticData GetWaveController() => null;
        public List<ShopItemStaticData> GetAllShopItems() => new List<ShopItemStaticData>();
        public HeroStaticData ForHero(HeroTypeId heroId) => null;
        public MapLevelLibrary GetMapLevelLibrary() => null;
        public AudioStaticData GetAudio() => null;
        public MusicStaticData GetMusic() => null;
        public VfxStaticData GetVfx() => null;
    }
}
