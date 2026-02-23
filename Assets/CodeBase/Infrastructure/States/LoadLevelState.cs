using System.Threading.Tasks;
using CodeBase.CameraLogic;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.ProgressService;
using CodeBase.Logic;
using CodeBase.Player;
using CodeBase.StaticData;
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
        private readonly IProgressService _progressService;

        public LoadLevelState(GameStateMachine gameStateMachine, SceneLoader sceneLoader, LoadingCurtain loadingCurtain,
            IGameFactory gameFactory, IPersistentProgressService persistentProgressService,
            IStaticDataService staticData, IProgressService progressService)
        {
            _staticData = staticData;
            _persistentProgressService = persistentProgressService;
            _gameFactory = gameFactory;
            _stateMachine = gameStateMachine;
            _sceneLoader = sceneLoader;
            _loadingCurtain = loadingCurtain;
            _progressService = progressService;
        }
        public void Enter(string sceneName)
        {
            _loadingCurtain.Show();
            _gameFactory.Cleanup();
            _gameFactory.WarmUp();
            _sceneLoader.Load(sceneName, OnLoaded);
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
            LevelStaticData levelData = LevelStaticData();
            GameObject player = await InitPlayer(levelData);
            await InitSpawner(levelData);
            await InitHud();
            CameraFollow(player);
        }
        
        private void CameraFollow(GameObject hero) =>
            Camera.main.GetComponent<CameraFollow>().Follow(hero);
        
        private async Task<GameObject> InitPlayer(LevelStaticData levelData)
        {
            GameObject player = await _gameFactory.CreatePlayer(levelData.initialHeroPosition);
            player.GetComponent<PlayerDeath>().Construct(_stateMachine);
            
            return player;
        }
        
        private async Task InitHud()
        {
            await _gameFactory.CreateHud();
        }
        
        private async Task InitSpawner(LevelStaticData levelData)
        {
                await _gameFactory.CreateSpawner(levelData.spawnersPosition);
        }

        
        private LevelStaticData LevelStaticData()
        {
            string sceneKey = SceneManager.GetActiveScene().name;
            LevelStaticData levelData = _staticData.ForLevel(sceneKey);
            return levelData;
        }
    }
}