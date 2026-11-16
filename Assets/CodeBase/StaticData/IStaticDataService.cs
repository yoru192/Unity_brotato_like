using System.Collections.Generic;
using CodeBase.Data;
using CodeBase.Infrastructure.Services;
using CodeBase.StaticData.Ability;
using CodeBase.StaticData.Enemy;
using CodeBase.StaticData.Hero;
using CodeBase.StaticData.Map;
using CodeBase.StaticData.Weapon;

namespace CodeBase.StaticData
{
    public interface IStaticDataService : IService
    {
        void Load();
        LevelStaticData ForLevel(string sceneKey);
        WeaponStaticData ForWeapon(WeaponTypeId weaponId);
        EnemyStaticData ForEnemy(EnemyTypeId enemyId);
        List<UpgradeStaticData> GetAllUpgrades();
        AbilityStaticData ForAbility(AbilityTypeId abilityId);
        WaveControllerStaticData GetWaveController();
        List<ShopItemStaticData> GetAllShopItems();
        HeroStaticData ForHero(HeroTypeId heroId);
        MapStaticData GetMapSettings();
        MapLevelLibrary GetMapLevelLibrary();
        AudioStaticData GetAudio();
        MusicStaticData GetMusic();
        Vfx.VfxStaticData GetVfx();

        /// <summary>Усі мета-апгрейди, відсортовані за гілкою та порядком у гілці.</summary>
        List<MetaUpgrade.MetaUpgradeStaticData> GetAllMetaUpgrades();
        MetaUpgrade.MetaUpgradeStaticData ForMetaUpgrade(MetaUpgrade.MetaUpgradeId id);
    }
}