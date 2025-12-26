using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.StaticData;
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
        Task<GameObject> CreateWeapon(WeaponTypeId weaponId, Transform parent);
        Task<GameObject> CreateEnemy(EnemyTypeId enemyId, Transform parent);
    }
}