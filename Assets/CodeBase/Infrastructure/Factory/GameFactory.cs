using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Enemy;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Logic;
using CodeBase.Player;
using CodeBase.StaticData;
using CodeBase.UI.Elements;
using CodeBase.Weapon;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace CodeBase.Infrastructure.Factory
{
    public class GameFactory : IGameFactory
    {
        public List<ISavedProgressReader> ProgressReaders { get; } = new List<ISavedProgressReader>();
        public List<ISavedProgress> ProgressWriters { get; } = new List<ISavedProgress>();
        public GameObject PlayerGameObject { get; set; }

        private readonly IAssets _assets;
        private readonly IStaticDataService _staticData;


        public GameFactory(IAssets assets, IStaticDataService staticData)
        {
            _assets = assets;
            _staticData = staticData;
        }

        public async Task WarmUp()
        {

        }

        public async Task<GameObject> CreatePlayer(Vector3 at)
        {
            PlayerGameObject = await InstantiateRegistered(AssetsAddress.PlayerPath, at);
            return PlayerGameObject;
        }

        public async Task<GameObject> CreateEnemy(EnemyTypeId enemyId, Transform parent)
        {
            EnemyStaticData enemyData = _staticData.ForEnemy(enemyId);
            GameObject prefab = await _assets.Load<GameObject>(enemyData.prefabReference);
            GameObject enemy = InstantiateRegisteredAsync(prefab, parent.transform.position);
    
            IHealth health = enemy.GetComponent<IHealth>();
            health.Current = enemyData.health;
            health.Max = enemyData.health;

            enemy.GetComponent<EnemyAttack>().Construct(
                PlayerGameObject.transform,
                enemy.GetComponent<EnemyAnimator>(),
                enemyData.cooldown,
                enemyData.radius,
                enemyData.effectiveDistance,
                enemyData.damage);
            enemy.GetComponent<ActorUI>().Construct(health);
            enemy.GetComponent<NavMeshAgent>().speed = enemyData.moveSpeed;
            enemy.GetComponent<AgentMoveToPlayer>().Construct(PlayerGameObject.transform);
           
    
            return enemy;
        }


        public async Task<GameObject> CreateWeapon(WeaponTypeId weaponId, Transform parent)
        {
            WeaponStaticData weaponData = _staticData.ForWeapon(weaponId);
            GameObject prefab = await _assets.Load<GameObject>(weaponData.prefabReference);
            GameObject weapon = Object.Instantiate(prefab, parent, false);
    
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
    
            WeaponAttack weaponAttack = weapon.GetComponent<WeaponAttack>();
            weaponAttack.Construct(
                weapon.GetComponent<WeaponAnimator>(),
                weaponData.effectiveDistance,
                weaponData.radius,
                weaponData.damage,
                weaponData.cooldown
            );
    
            return weapon;
        }

        public async Task<GameObject> CreateSpawner(List<Vector2> spawnPositions)
        {
            GameObject spawner = await InstantiateRegistered(AssetsAddress.SpawnerPath, Vector3.zero);
    
            EnemySpawner enemySpawner = spawner.GetComponent<EnemySpawner>();
            if (enemySpawner != null)
                enemySpawner.Construct(this, spawnPositions);
    
            return spawner;
        }


        public void Cleanup()
        {
            ProgressReaders.Clear();
            ProgressWriters.Clear();
        }
        
        public void Register(ISavedProgressReader progressReader)
        {
            if (progressReader is ISavedProgress progressWriter)
                ProgressWriters.Add(progressWriter);
            
            ProgressReaders.Add(progressReader);
        }

        private void RegisterProgressWatchers(GameObject gameObject)
        {
            foreach (ISavedProgressReader progressReader  in gameObject.GetComponentsInChildren<ISavedProgressReader>())
                Register(progressReader);
        }
        private GameObject InstantiateRegisteredAsync(GameObject prefab, Vector3 position)
        {
            GameObject gameObject = Object.Instantiate(prefab, position, Quaternion.identity);
            RegisterProgressWatchers(gameObject);
            return gameObject;
        }

        private GameObject InstantiateRegisteredAsync(GameObject prefab)
        {
            GameObject gameObject = Object.Instantiate(prefab);
            RegisterProgressWatchers(gameObject);
            return gameObject;
        }

        private async Task<GameObject> InstantiateRegistered(string prefabPath, Vector3 position)
        {
            GameObject gameObject = await _assets.Instantiate(prefabPath, position);
            RegisterProgressWatchers(gameObject);
            return gameObject;
        }

        private async Task<GameObject> InstantiateRegistered(string prefabPath)
        {
            GameObject gameObject = await _assets.Instantiate(prefabPath);
            RegisterProgressWatchers(gameObject);
            return gameObject;
        }
    }
}