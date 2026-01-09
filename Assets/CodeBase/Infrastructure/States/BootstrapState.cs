using System.ComponentModel;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Inputs;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.Infrastructure.Services.Upgrade;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Infrastructure.States
{
    public class BootstrapState : IState
    {
        private const string Initial = "Initial";
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly AllServices _services;

        public BootstrapState(GameStateMachine stateMachine, SceneLoader sceneLoader, AllServices services)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _services = services;
            
            RegisterServices();
        }
        
        public void Enter()
        {
            _sceneLoader.Load(Initial, onLoaded: EnterLoadLevel);
        }

        public void Exit()
        {
        }

        private void EnterLoadLevel()
        {
            _stateMachine.Enter<LoadProgressState>();
        }

        private void RegisterServices()
        {
            RegisterStaticData();
            
            _services.RegisterSingle<IGameStateMachine>(_stateMachine);
            RegisterAssetsProvider();
            
            _services.RegisterSingle<IInputService>(InputService());
            _services.RegisterSingle<IPersistentProgressService>( new PersistentProgressService());
            _services.RegisterSingle<IProgressService>(new ProgressService(_services.Single<IPersistentProgressService>()));
            _services.RegisterSingle<IGameFactory>(new GameFactory(
                _services.Single<IAssets>(), _services.Single<IStaticDataService>(), _services.Single<IProgressService>(), _services.Single<IPersistentProgressService>()));
            _services.RegisterSingle<IUpgradeService>(new UpgradeService(_services.Single<IStaticDataService>(), _services.Single<IPersistentProgressService>()));
            _services.RegisterSingle<ISaveLoadService>( new SaveLoadService(_services.Single<IPersistentProgressService>(), _services.Single<IGameFactory>()));
        }
        
        private void RegisterAssetsProvider()
        {
            AssetsProvider assetsProvider = new AssetsProvider();
            assetsProvider.Instantiate();
            _services.RegisterSingle<IAssets>(assetsProvider);
        }
        private void RegisterStaticData()
        {
            IStaticDataService staticData = new StaticDataService();
            staticData.Load();
            _services.RegisterSingle(staticData);
        }
        
        private static IInputService InputService()
        {
            if (Application.isEditor)
                return new StandaloneInputService();
            else
                return new MobileInputService();
            
        }
    }
}