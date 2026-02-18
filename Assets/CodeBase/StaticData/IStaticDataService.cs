using System.Collections.Generic;
using CodeBase.Data;
using CodeBase.Infrastructure.Services;
using CodeBase.StaticData.Enemy;
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
        PlayerStaticData GetPlayer();
        WaveControllerStaticData GetWaveController();
    }
}