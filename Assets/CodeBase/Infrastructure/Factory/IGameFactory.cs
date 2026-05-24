using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Logic;
using CodeBase.StaticData;
using CodeBase.StaticData.Enemy;
using CodeBase.StaticData.Hero;
using CodeBase.StaticData.Weapon;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public interface IGameFactory : IService
    {
        List<ISavedProgressReader> ProgressReaders { get; }
        List<ISavedProgress> ProgressWriters { get; }
        WaveController WaveController { get; }
        PauseInputHandler PauseInputHandler { get; }
        Task WarmUp();
        void Cleanup();
        Task<GameObject> CreatePlayer(Vector3 at, HeroTypeId heroType);
        Task<GameObject> CreateEnemy(EnemyTypeId enemyId, Vector2 position);
        Task<GameObject> CreateSpawner(List<Vector2> spawnPositions);
        Task<GameObject> CreateHud();
        Task<GameObject> CreateGameOverScreen();
        Task<GameObject> CreateWinScreen();
        Task<GameObject> CreatePauseScreen();
        Task<GameObject> CreateUpgradeScreen();
        Task<GameObject> CreateShopScreen();
        Task EquipWeapon(WeaponTypeId weaponId);
    }
}