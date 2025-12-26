using System.Threading.Tasks;
using CodeBase.CameraLogic;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Logic;
using CodeBase.StaticData;
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
        private readonly IPersistentProgressService _progressService;
        private readonly IStaticDataService _staticData;

        public LoadLevelState(GameStateMachine gameStateMachine, SceneLoader sceneLoader, LoadingCurtain loadingCurtain,
            IGameFactory gameFactory, IPersistentProgressService progressService, IStaticDataService staticData)
        {
            _staticData = staticData;
            _progressService = progressService;
            _gameFactory = gameFactory;
            _stateMachine = gameStateMachine;
            _sceneLoader = sceneLoader;
            _loadingCurtain = loadingCurtain;
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
                progressReader.LoadProgress(_progressService.Progress);
        }

        private async Task InitGameWorld()
        {
            LevelStaticData levelData = LevelStaticData();
            GameObject player = await InitPlayer(levelData);
            CameraFollow(player);
        }
        
        
        
        private void CameraFollow(GameObject hero) =>
            Camera.main.GetComponent<CameraFollow>().Follow(hero);
        
        private async Task<GameObject> InitPlayer(LevelStaticData levelData)
        {
            GameObject player = await _gameFactory.CreatePlayer(levelData.initialHeroPosition);

            WeaponHolder weaponHolder = player.GetComponentInChildren<WeaponHolder>();
            if (weaponHolder != null)
                weaponHolder.Construct(_gameFactory);
    
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