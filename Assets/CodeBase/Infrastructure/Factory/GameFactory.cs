using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Ability;
using CodeBase.Ability.Concrete;
using CodeBase.Enemy;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services.Balance;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.ProgressService;
using CodeBase.Infrastructure.Services.Upgrade;
using CodeBase.Logic;
using CodeBase.Player;
using CodeBase.Player.Movement;
using CodeBase.StaticData;
using CodeBase.StaticData.Ability;
using CodeBase.StaticData.Enemy;
using CodeBase.StaticData.Weapon;
using CodeBase.UI;
using CodeBase.UI.Elements;
using CodeBase.Weapon;
using CodeBase.Weapon.RangeWeapon;
using Pathfinding;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public class GameFactory : IGameFactory
    {
        public List<ISavedProgressReader> ProgressReaders { get; } = new();
        public List<ISavedProgress> ProgressWriters { get; } = new();
        public GameObject PlayerGameObject { get; set; }

        private readonly IAssets _assets;
        private readonly IStaticDataService _staticData;
        private readonly IProgressService _progress;
        private readonly IPersistentProgressService _persistentProgress;
        private readonly IUpgradeService _upgradeService;
        private readonly IBalanceService _balanceService;

        public GameFactory(IAssets assets, IStaticDataService staticData, IBalanceService balanceService,
            IProgressService progress, IPersistentProgressService progressService, IUpgradeService upgradeService)
        {
            _assets = assets;
            _staticData = staticData;
            _balanceService = balanceService;
            _progress = progress;
            _persistentProgress = progressService;
            _upgradeService = upgradeService;
        }

        public async Task WarmUp() { }
        

        public async Task<GameObject> CreatePlayer(Vector3 at)
        {
            PlayerGameObject = await InstantiateRegistered(AssetsAddress.PlayerPath, at);

            PlayerStaticData playerData = _staticData.GetPlayer();

            PlayerGameObject.GetComponent<PlayerMovement>().Construct(_persistentProgress, playerData);
            PlayerGameObject.GetComponent<PlayerStamina>().Construct(_persistentProgress, _upgradeService, playerData);
            PlayerGameObject.GetComponent<PlayerHealth>().Construct(playerData, _persistentProgress);
            
            WeaponHolder weaponHolder = PlayerGameObject.GetComponentInChildren<WeaponHolder>();
            foreach (WeaponTypeId weaponId in playerData.startWeapons)
                await CreateAndEquipWeapon(weaponHolder, weaponId);
            
            AbilityController abilityController = PlayerGameObject.AddComponent<AbilityController>();
            PlayerAbilityInput abilityInput = PlayerGameObject.AddComponent<PlayerAbilityInput>();

            Rigidbody2D playerRb = PlayerGameObject.GetComponent<Rigidbody2D>();
            PlayerMovement playerMovement = PlayerGameObject.GetComponent<PlayerMovement>();

            DashAbilityData dashData = _staticData.ForAbility(AbilityTypeId.Dash) as DashAbilityData;
            if (dashData != null)
            {
                DashAbility dashAbility = new DashAbility(
                    PlayerGameObject.transform, playerRb, playerMovement, dashData);
                abilityController.AddAbility(dashAbility);
            }

            abilityInput.Construct(abilityController);
            return PlayerGameObject;
        }
        
        private async Task CreateAndEquipWeapon(WeaponHolder holder, WeaponTypeId weaponId)
        {
            WeaponStaticData data = _staticData.ForWeapon(weaponId);
            GameObject prefab = await _assets.Load<GameObject>(data.prefabReference);
            GameObject weaponGo = Object.Instantiate(prefab, holder.transform);
            
            RegisterProgressWatchers(weaponGo);

            switch (weaponId)
            {
                case WeaponTypeId.Melee:
                    weaponGo.GetComponent<MeleeWeapon>()
                            .Construct(data.damage, data.cooldown, data.radius, data.attackAngle);
                    break;

                case WeaponTypeId.Ranged:
                    weaponGo.GetComponent<RangedAttack>()
                            .Construct(data.damage, data.shootRate, data.projectileSpeed, data.radius);
                    break;
            }

            holder.AddWeapon(weaponId, weaponGo.GetComponent<WeaponBase>());
        }
        public async Task<GameObject> CreateHud()
        {
            WeaponStaticData weaponData = _staticData.ForWeapon(WeaponTypeId.Melee);
            GameObject hud = await InstantiateRegistered(AssetsAddress.HudPath);
            hud.GetComponent<HudUI>().Construct(
                _balanceService,
                _progress,
                _persistentProgress,
                PlayerGameObject.GetComponent<PlayerStamina>(),
                PlayerGameObject.GetComponent<PlayerHealth>(),
                weaponData,
                _upgradeService,
                _staticData.GetPlayer()
            );
            return hud;
        }

        public async Task<GameObject> CreateGameOverScreen()
        {
            return await InstantiateRegistered(AssetsAddress.GameOverScreenPath);
        }

        public async Task<GameObject> CreateUpgradeScreen()
        {
            return await InstantiateRegistered(AssetsAddress.UpgradeScreenPath);
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
                kamikadzeAttack.Construct(PlayerGameObject.transform, enemyData.damage);

            if (enemy.TryGetComponent<EnemyAttack>(out var attack))
                attack.Construct(PlayerGameObject.transform, enemy.GetComponent<EnemyAnimator>(),
                    enemyData.cooldown, enemyData.radius, enemyData.damage);
            if(enemy.TryGetComponent<EnemyRangerAttack>(out var rangerAttack))
                rangerAttack.Construct(enemyData.damage, enemyData.cooldown, enemyData.projectileSpeed, enemyData.radius);

            enemy.GetComponent<EnemyDeath>().Construct(_balanceService, _progress, enemyData);
            enemy.GetComponent<ActorUI>().Construct(health);
            enemy.GetComponent<AIDestinationSetter>().target = PlayerGameObject.transform;
            enemy.GetComponentInChildren<EnemyGFX>().Construct(enemyData.spriteScale);

            return enemy;
        }
        
        public async Task<GameObject> CreateSpawner(List<Vector2> spawnPositions)
        {
            GameObject spawner = await InstantiateRegistered(AssetsAddress.SpawnerPath, Vector3.zero);
            spawner.GetComponent<WaveController>().Construct(_staticData.GetWaveController());

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
            foreach (ISavedProgressReader reader in gameObject.GetComponentsInChildren<ISavedProgressReader>())
                Register(reader);
        }

        private GameObject InstantiateRegisteredAsync(GameObject prefab, Vector3 position)
        {
            GameObject go = Object.Instantiate(prefab, position, Quaternion.identity);
            RegisterProgressWatchers(go);
            return go;
        }

        private GameObject InstantiateRegisteredAsync(GameObject prefab)
        {
            GameObject go = Object.Instantiate(prefab);
            RegisterProgressWatchers(go);
            return go;
        }

        private async Task<GameObject> InstantiateRegistered(string prefabPath, Vector3 position)
        {
            GameObject go = await _assets.Instantiate(prefabPath, position);
            RegisterProgressWatchers(go);
            return go;
        }

        private async Task<GameObject> InstantiateRegistered(string prefabPath)
        {
            GameObject go = await _assets.Instantiate(prefabPath);
            RegisterProgressWatchers(go);
            return go;
        }
    }
}