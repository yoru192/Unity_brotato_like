using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Ability;
using CodeBase.Ability.Concrete;
using CodeBase.Enemy;
using CodeBase.Loot;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services.Balance;
using CodeBase.Infrastructure.Services.Buff;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.ProgressService;
using CodeBase.Infrastructure.Services.SelectedLevel;
using CodeBase.Infrastructure.Services.ShopService;
using CodeBase.Infrastructure.Services.Upgrade;
using CodeBase.Logic;
using CodeBase.Player;
using CodeBase.Player.Movement;
using CodeBase.StaticData;
using CodeBase.StaticData.Ability;
using CodeBase.StaticData.Enemy;
using CodeBase.StaticData.Hero;
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
        public WaveController WaveController { get; private set; }
        public PauseInputHandler PauseInputHandler { get; private set; }

        private readonly IAssets _assets;
        private readonly IStaticDataService _staticData;
        private readonly IProgressService _progress;
        private readonly IPersistentProgressService _persistentProgress;
        private readonly IUpgradeService _upgradeService;
        private readonly IBalanceService _balanceService;
        private readonly IShopService _shopService;
        private readonly IBuffService _buffService;
        private readonly ISelectedLevelService _selectedLevel;

        public GameFactory(IAssets assets,
            IStaticDataService staticData,
            IBalanceService balanceService,
            IProgressService progress,
            IPersistentProgressService progressService,
            IUpgradeService upgradeService,
            IShopService shopService,
            IBuffService buffService,
            ISelectedLevelService selectedLevel)
        {
            _assets = assets;
            _staticData = staticData;
            _balanceService = balanceService;
            _progress = progress;
            _persistentProgress = progressService;
            _upgradeService = upgradeService;
            _shopService = shopService;
            _buffService = buffService;
            _selectedLevel = selectedLevel;
        }

        public async Task WarmUp()
        {
        }


        public async Task<GameObject> CreatePlayer(Vector3 at, HeroTypeId heroType)
        {
            PlayerGameObject = await InstantiateRegistered(AssetsAddress.PlayerPath, at);
            
            HeroStaticData   heroData   = _staticData.ForHero(heroType);
            PlayerGameObject.GetComponent<PlayerMovement>().Construct(heroData);
            PlayerGameObject.GetComponent<PlayerStamina>()
                .Construct(_persistentProgress, _upgradeService, heroData);
            PlayerGameObject.GetComponent<PlayerHealth>().Construct(heroData, _persistentProgress);

            WeaponHolder weaponHolder = PlayerGameObject.GetComponentInChildren<WeaponHolder>();
            // First run uses the hero's starter weapons; later runs re-equip everything owned
            // (starters + anything bought at a shop node), which CreateAndEquipWeapon persists.
            List<WeaponTypeId> ownedWeapons = _persistentProgress.Progress.playerState.OwnedWeapons;
            List<WeaponTypeId> weaponsToEquip = ownedWeapons.Count > 0
                ? new List<WeaponTypeId>(ownedWeapons)
                : new List<WeaponTypeId>(heroData.startWeapons);
            foreach (WeaponTypeId weaponId in weaponsToEquip)
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

            foreach (ISavedProgressReader reader in weaponGo.GetComponentsInChildren<ISavedProgressReader>())
                reader.LoadProgress(_persistentProgress.Progress);

            holder.AddWeapon(weaponId, weaponGo.GetComponent<WeaponBase>());

            var ownedWeapons = _persistentProgress.Progress.playerState.OwnedWeapons;
            if (!ownedWeapons.Contains(weaponId))
                ownedWeapons.Add(weaponId);
        }

        public async Task EquipWeapon(WeaponTypeId weaponId)
        {
            WeaponHolder holder = PlayerGameObject.GetComponentInChildren<WeaponHolder>();
            await CreateAndEquipWeapon(holder, weaponId);
        }

        public async Task<GameObject> CreateHud()
        {
            WeaponStaticData meleeWeaponData = _staticData.ForWeapon(WeaponTypeId.Melee);
            WeaponStaticData rangedWeaponData = _staticData.ForWeapon(WeaponTypeId.Ranged);
            GameObject hud = await InstantiateRegistered(AssetsAddress.HudPath);
            PauseInputHandler = hud.AddComponent<PauseInputHandler>();
            HudUI hudUI = hud.GetComponent<HudUI>();
            hudUI.Construct(
                _balanceService,
                _progress,
                _persistentProgress,
                PlayerGameObject.GetComponent<PlayerStamina>(),
                PlayerGameObject.GetComponent<PlayerHealth>(),
                meleeWeaponData,
                rangedWeaponData,
                _upgradeService,
                _staticData,
                _buffService
            );

            if (WaveController != null)
                hudUI.SetWaveController(WaveController);

            return hud;
        }

        public async Task<GameObject> CreateUpgradeScreen()
        {
            return await InstantiateRegistered(AssetsAddress.UpgradeScreenPath);
        }

        public async Task<GameObject> CreateShopScreen()
        {
            return await InstantiateRegistered(AssetsAddress.ShopScreenPath);
        }

        public async Task<GameObject> CreateCampfireScreen()
        {
            return await InstantiateRegistered(AssetsAddress.CampfireScreenPath);
        }

        public async Task<GameObject> CreateGameOverScreen()
        {
            return await InstantiateRegistered(AssetsAddress.GameOverScreenPath);
        }

        public async Task<GameObject> CreateWinScreen()
        {
            return await InstantiateRegistered(AssetsAddress.WinScreenPath);
        }

        public async Task<GameObject> CreatePauseScreen()
        {
            return await InstantiateRegistered(AssetsAddress.PauseScreenPath);
        }

        public async Task<GameObject> CreateEnemy(EnemyTypeId enemyId, Vector2 position)
        {
            EnemyStaticData enemyData = _staticData.ForEnemy(enemyId);
            GameObject prefab = await _assets.Load<GameObject>(enemyData.prefabReference);
            XpOrbSet xpOrbSet = await LoadXpOrbSet();

            bool isFirstSpawn = !ObjectPoolManager.HasPool(prefab);

            GameObject enemy = ObjectPoolManager.SpawnObject(
                prefab,
                position,
                Quaternion.identity,
                ObjectPoolManager.PoolType.Enemy
            );

            if (isFirstSpawn || !enemy.GetComponent<EnemyDeath>().IsConstructed)
            {
                SetupEnemy(enemy, enemyData, xpOrbSet);
            }
            
            IHealth health = enemy.GetComponent<IHealth>();
            health.Current = enemyData.health;
            health.Max = enemyData.health;

            enemy.GetComponent<AIDestinationSetter>().target = PlayerGameObject.transform;
            
            foreach (var poolable in enemy.GetComponentsInChildren<IPoolable>())
                poolable.OnSpawn();

            return enemy;
        }

        private async Task<XpOrbSet> LoadXpOrbSet()
        {
            GameObject large = await _assets.Load<GameObject>(AssetsAddress.XpOrbLargePath);
            GameObject medium = await _assets.Load<GameObject>(AssetsAddress.XpOrbMediumPath);
            GameObject small = await _assets.Load<GameObject>(AssetsAddress.XpOrbSmallPath);
            return new XpOrbSet(large.GetComponent<XpOrb>(), medium.GetComponent<XpOrb>(), small.GetComponent<XpOrb>());
        }

        private void SetupEnemy(GameObject enemy, EnemyStaticData enemyData, XpOrbSet xpOrbSet)
        {
            RegisterProgressWatchers(enemy);

            if (enemy.TryGetComponent<EnemyKamikadzeAttack>(out var kamikadze))
                kamikadze.Construct(PlayerGameObject.transform, enemyData.damage, enemyData.radius);

            if (enemy.TryGetComponent<EnemyAttack>(out var attack))
                attack.Construct(PlayerGameObject.transform, enemyData.cooldown, enemyData.radius, enemyData.damage);

            if (enemy.TryGetComponent<EnemyRangerAttack>(out var ranger))
                ranger.Construct(enemyData.damage, enemyData.cooldown, enemyData.projectileSpeed, enemyData.radius);

            enemy.GetComponent<EnemyDeath>().Construct(_balanceService, _progress, enemyData, xpOrbSet, PlayerGameObject.transform);
            enemy.GetComponent<ActorUI>().Construct(enemy.GetComponent<IHealth>());
            enemy.GetComponentInChildren<EnemyGFX>().Construct(enemyData.spriteScale);

            enemy.GetComponent<EnemyDeath>().IsConstructed = true;
        }

        public async Task<GameObject> CreateSpawner(List<Vector2> spawnPositions)
        {
            GameObject spawner = await InstantiateRegistered(AssetsAddress.SpawnerPath, Vector3.zero);
            WaveController = spawner.GetComponent<WaveController>();
            // Use the wave config of the node picked on the level map; fall back to the global
            // config when launched outside the map flow (e.g. direct play in the editor).
            WaveControllerStaticData waveConfig = _selectedLevel.SelectedWaveConfig ?? _staticData.GetWaveController();
            WaveController.Construct(waveConfig);

            EnemySpawner enemySpawner = spawner.GetComponent<EnemySpawner>();
            if (enemySpawner != null)
                enemySpawner.Construct(this, spawnPositions, PlayerGameObject.transform);

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