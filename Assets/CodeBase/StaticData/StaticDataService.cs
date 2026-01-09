using System.Collections.Generic;
using System.Linq;
using CodeBase.Data;
using UnityEngine;

namespace CodeBase.StaticData
{
    public class StaticDataService : IStaticDataService
    {
        private Dictionary<string, LevelStaticData> _levels;
        private Dictionary<WeaponTypeId, WeaponStaticData> _weapons;
        private Dictionary<EnemyTypeId, EnemyStaticData> _enemies;
        private List<UpgradeStaticData> _upgrades;
        

        public void Load()
        {
            _levels = Resources.LoadAll<LevelStaticData>("StaticData/Levels")
                .ToDictionary(x => x.levelKey,  x => x);
            _weapons = Resources.LoadAll<WeaponStaticData>("StaticData/Weapons")
                .ToDictionary(x => x.weaponTypeId,  x => x);
            _enemies = Resources.LoadAll<EnemyStaticData>("StaticData/Enemy")
                .ToDictionary(x => x.enemyTypeId,  x => x);
            _upgrades = Resources.LoadAll<UpgradeStaticData>("StaticData/Upgrades")
                .ToList();
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

        public List<UpgradeStaticData> GetAllUpgrades() => _upgrades;
    }
}