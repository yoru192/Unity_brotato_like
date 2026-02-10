using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Data;
using CodeBase.Enemy;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.ProgressService;
using CodeBase.Infrastructure.States;
using CodeBase.Logic;
using CodeBase.Player;
using CodeBase.Player.Movement;
using CodeBase.StaticData;
using CodeBase.UI;
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
        private readonly IProgressService _progress;
        private IPersistentProgressService _persistentProgress;

        public GameFactory(IAssets assets, IStaticDataService staticData, IProgressService progress,
            IPersistentProgressService progressService)
        {
            _assets = assets;
            _staticData = staticData;
            _progress = progress;
            _persistentProgress = progressService;
        }

        public async Task WarmUp()
        {

        }

        public async Task<GameObject> CreatePlayer(Vector3 at)
        {
            PlayerGameObject = await InstantiateRegistered(AssetsAddress.PlayerPath, at);
            PlayerGameObject.GetComponent<PlayerMovement>().Construct(_persistentProgress);
    
            WeaponStaticData weaponData = _staticData.ForWeapon(WeaponTypeId.Melee);
            WeaponAttack weaponAttack = PlayerGameObject.GetComponentInChildren<WeaponAttack>();
    
            weaponAttack.Construct(
                PlayerGameObject.GetComponentInChildren<WeaponAnimator>(),
                weaponData.effectiveDistance,
                weaponData.radius,
                weaponData.damage,
                weaponData.cooldown
            );
            
            PlayerGameObject.GetComponent<PlayerStamina>().Construct(_persistentProgress);
            return PlayerGameObject;
        }

        
        public async Task<GameObject> CreateHud()
        {
            GameObject hud = await InstantiateRegistered(AssetsAddress.HudPath);
            hud.GetComponent<HudUI>().Construct(
                _progress, 
                _persistentProgress, 
                PlayerGameObject.GetComponent<PlayerStamina>()
            );
            hud.GetComponentInChildren<ActorUI>().Construct(PlayerGameObject.GetComponent<PlayerHealth>());
            return hud;
        }
        
        public async Task<GameObject> CreateGameOverScreen()
        {
            GameObject gameOverScreen = await InstantiateRegistered(AssetsAddress.GameOverScreenPath);
            return gameOverScreen;
        }

        public async Task<GameObject> CreateUpgradeScreen()
        {
            GameObject upgradeScreen = await InstantiateRegistered(AssetsAddress.UpgradeScreenPath);
            return upgradeScreen;
        }


        public async Task<GameObject> CreateEnemy(EnemyTypeId enemyId, Transform parent)
        {
            EnemyStaticData enemyData = _staticData.ForEnemy(enemyId);
            GameObject prefab = await _assets.Load<GameObject>(enemyData.prefabReference);
            GameObject enemy = InstantiateRegisteredAsync(prefab, parent.transform.position);
    
            IHealth health = enemy.GetComponent<IHealth>();
            health.Current = enemyData.health;
            health.Max = enemyData.health;

            if (enemy.TryGetComponent<EnemyKamikadzeAttack>(out var kamikadzeAttack))
            {
                kamikadzeAttack.Construct(PlayerGameObject.transform, enemyData.damage);
            }
            if (enemy.TryGetComponent<EnemyAttack>(out var attack))
            {
                attack.Construct(
                    PlayerGameObject.transform,
                    enemy.GetComponent<EnemyAnimator>(),
                    enemyData.cooldown,
                    enemyData.effectiveDistance,
                    enemyData.damage);
            }
            enemy.GetComponent<EnemyDeath>().Construct(_progress,enemyData);
            enemy.GetComponent<ActorUI>().Construct(health);
            enemy.GetComponent<NavMeshAgent>().speed = enemyData.moveSpeed;
            enemy.GetComponent<AgentMoveToPlayer>().Construct(PlayerGameObject.transform);
           
    
            return enemy;
        }


        public async Task CreateWeapon(WeaponTypeId weaponId, Transform parent)
        {
            WeaponStaticData weaponData = _staticData.ForWeapon(weaponId);
            //GameObject prefab = await _assets.Load<GameObject>(weaponData.prefabReference);
            //GameObject weapon = InstantiateRegisteredAsync(prefab);
    
            //weapon.transform.SetParent(parent, false);
            //weapon.transform.localPosition = Vector3.zero;
            //weapon.transform.localRotation = Quaternion.identity;
    
            WeaponAttack weaponAttack = PlayerGameObject.GetComponentInChildren<WeaponAttack>();
            weaponAttack.Construct(
                PlayerGameObject.GetComponentInChildren<WeaponAnimator>(),
                weaponData.effectiveDistance,
                weaponData.radius,
                weaponData.damage,
                weaponData.cooldown
            );
            
            IPersistentProgressService progressService = _persistentProgress as IPersistentProgressService;
            if (progressService != null)
            {
                weaponAttack.LoadProgress(progressService.Progress);
            }
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