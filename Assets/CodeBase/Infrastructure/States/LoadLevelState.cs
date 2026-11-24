using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.CameraLogic;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.ProgressService;
using CodeBase.Logic;
using CodeBase.Logic.Arena;
using CodeBase.Player;
using CodeBase.StaticData;
using CodeBase.StaticData.Hero;
using CodeBase.UI;
using CodeBase.UI.Elements;
using CodeBase.Weapon;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeBase.Infrastructure.States
{
    public class LoadLevelState : IPayloadedState<string>
    {
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly LoadingCurtain _loadingCurtain;
        private readonly IGameFactory _gameFactory;
        private readonly IPersistentProgressService _persistentProgressService;
        private readonly IStaticDataService _staticData;

        public LoadLevelState(GameStateMachine gameStateMachine, SceneLoader sceneLoader, LoadingCurtain loadingCurtain,
            IGameFactory gameFactory, IPersistentProgressService persistentProgressService,
            IStaticDataService staticData)
        {
            _staticData = staticData;
            _persistentProgressService = persistentProgressService;
            _gameFactory = gameFactory;
            _stateMachine = gameStateMachine;
            _sceneLoader = sceneLoader;
            _loadingCurtain = loadingCurtain;
        }
        public void Enter(string sceneName)
        {
            Debug.Log("Entering LoadLevelState");
            _loadingCurtain.Show();
            _gameFactory.Cleanup();
            _gameFactory.WarmUp();
            _sceneLoader.Load(sceneName, OnLoaded, _loadingCurtain.SetProgress);
        }

        public void Exit() => 
            _loadingCurtain.Hide();

        private async void OnLoaded()
        {
            await InitGameWorld();
            InformProgressReaders();
            _stateMachine.Enter<GameLoopState>();
        }
        
        private void InformProgressReaders()
        {
            foreach (ISavedProgressReader progressReader in _gameFactory.ProgressReaders)
                progressReader.LoadProgress(_persistentProgressService.Progress);
        }

        private async Task InitGameWorld()
        {
            HeroTypeId heroType = _persistentProgressService.Progress.worldData.selectedHero;
            CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();

            // Legacy scenes (Main) have the map baked in and a LevelStaticData keyed by scene name;
            // every other combat scene builds its battlefield from an arena layout prefab.
            LevelStaticData levelData = LevelStaticData();
            if (levelData != null)
            {
                GameObject legacyPlayer = await InitPlayer(levelData.initialHeroPosition, heroType);
                await _gameFactory.CreateSpawner(levelData.spawnersPosition);
                await _gameFactory.CreateHud();
                cameraFollow.Follow(legacyPlayer);
                return;
            }

            ArenaLayout arena = await _gameFactory.CreateArena(AssetsAddress.DefaultArenaPath);
            arena.Build();
            cameraFollow.SetBounds(arena.Bounds);

            GameObject player = await InitPlayer(arena.HeroPosition, heroType);
            GameObject spawner = await _gameFactory.CreateSpawner(new List<Vector2>());
            spawner.GetComponent<EnemySpawner>().SpawnOffscreen(arena.Bounds, Camera.main);
            await _gameFactory.CreateHud();
            cameraFollow.Follow(player);
        }

        private async Task<GameObject> InitPlayer(Vector3 at, HeroTypeId heroType)
        {
            GameObject player = await _gameFactory.CreatePlayer(at, heroType);
            player.GetComponent<PlayerDeath>().Construct(_stateMachine);

            return player;
        }

        
        private LevelStaticData LevelStaticData()
        {
            string sceneKey = SceneManager.GetActiveScene().name;
            LevelStaticData levelData = _staticData.ForLevel(sceneKey);
            return levelData;
        }
    }
}