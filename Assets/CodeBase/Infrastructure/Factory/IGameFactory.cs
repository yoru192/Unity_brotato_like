using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.StaticData;
using CodeBase.StaticData.Enemy;
using CodeBase.StaticData.Weapon;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public interface IGameFactory : IService
    {
        List<ISavedProgressReader> ProgressReaders { get; }
        List<ISavedProgress> ProgressWriters { get; }
        Task WarmUp();
        void Cleanup();
        Task<GameObject> CreatePlayer(Vector3 at);
        Task CreateWeapon(WeaponTypeId weaponId, Transform parent);
        Task<GameObject> CreateEnemy(EnemyTypeId enemyId, Transform parent);
        Task<GameObject> CreateSpawner(List<Vector2> spawnPositions);
        Task<GameObject> CreateHud();
        Task<GameObject> CreateGameOverScreen();
        Task<GameObject> CreateUpgradeScreen();
    }
}