using System.Collections;
using System.ComponentModel;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Audio;
using CodeBase.Infrastructure.Services.Balance;
using CodeBase.Infrastructure.Services.Buff;
using CodeBase.Infrastructure.Services.CritDamage;
using CodeBase.Infrastructure.Services.HitFeel;
using CodeBase.Infrastructure.Services.Inputs;
using CodeBase.Infrastructure.Services.Map;
using CodeBase.Infrastructure.Services.MetaProgress;
using CodeBase.Infrastructure.Services.Music;
using CodeBase.Infrastructure.Services.Perk;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.ProgressService;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.Infrastructure.Services.SelectedLevel;
using CodeBase.Infrastructure.Services.Settings;
using CodeBase.Infrastructure.Services.ShopService;
using CodeBase.Infrastructure.Services.Upgrade;
using CodeBase.Infrastructure.Services.Vfx;
using CodeBase.Player;
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
        private ICoroutineRunner _coroutineRunner;

        public BootstrapState(GameStateMachine stateMachine, SceneLoader sceneLoader, AllServices services,
            ICoroutineRunner coroutineRunner)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _services = services;
            _coroutineRunner = coroutineRunner;
            
            RegisterServices();
        }
        
        public void Enter()
        {
            Debug.Log("Entering BootstrapState");
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
            
            ISettingsService settings = new SettingsService();
            settings.Load();
            _services.RegisterSingle(settings);
            // Сервіси реєструються в Awake, а AudioMixer.SetFloat до першого кадру не спрацьовує —
            // тому збережену гучність застосовуємо ще раз на наступному кадрі.
            _coroutineRunner.StartCoroutine(ApplyAudioSettingsNextFrame(settings));
            
            _services.RegisterSingle<IGameStateMachine>(_stateMachine);
            RegisterAssetsProvider();
            _services.RegisterSingle<IMusicService>(new MusicService(_services.Single<IStaticDataService>().GetMusic(), _coroutineRunner));
            _services.RegisterSingle<IInputService>(InputService());
            _services.RegisterSingle<IPersistentProgressService>( new PersistentProgressService());

            // Прогрес між забігами: читаємо з диска один раз на старті гри.
            // Перки реєструються раніше, бо дерево мета-апгрейдів видає стартові перки.
            _services.RegisterSingle<IPerkService>(new PerkService(_services.Single<IPersistentProgressService>()));
            IMetaProgressService metaProgress = new MetaProgressService(
                _services.Single<IStaticDataService>(),
                _services.Single<IPersistentProgressService>(),
                _services.Single<IPerkService>());
            metaProgress.Load();
            _services.RegisterSingle(metaProgress);
            _services.RegisterSingle<IProgressService>(new ProgressService(_services.Single<IPersistentProgressService>()));
            _services.RegisterSingle<IBalanceService>(new BalanceService(_services.Single<IPersistentProgressService>()));
            _services.RegisterSingle<IUpgradeService>(new UpgradeService(
                _services.Single<IStaticDataService>(),
                _services.Single<IPersistentProgressService>(),
                _services.Single<IPerkService>()));
            _services.RegisterSingle<IBuffService>(new BuffService(
                _coroutineRunner,
                _services.Single<IPersistentProgressService>()));
            _services.RegisterSingle<IShopService>(new ShopService(
                _services.Single<IStaticDataService>(),
                _services.Single<IUpgradeService>(),
                _coroutineRunner,
                _services.Single<IBuffService>(),
                _services.Single<IPersistentProgressService>()));
            _services.RegisterSingle<ISelectedLevelService>(new SelectedLevelService());
            _services.RegisterSingle<IMapService>(new MapService(
                _services.Single<IStaticDataService>(),
                _services.Single<IPersistentProgressService>(),
                _services.Single<ISelectedLevelService>()));
            _services.RegisterSingle<ICritDamageService>(new CritDamageService());
            _services.RegisterSingle<IHitFeelService>(new HitFeelService(_coroutineRunner));
            _services.RegisterSingle<IVfxService>(new VfxService(_services.Single<IStaticDataService>()));
            _services.RegisterSingle<IAudioService>(new AudioService(_services.Single<IStaticDataService>().GetAudio()));
            _services.RegisterSingle<IGameFactory>(new GameFactory(
                _services.Single<IAssets>(),
                _services.Single<IStaticDataService>(),
                _services.Single<IBalanceService>(),
                _services.Single<IProgressService>(),
                _services.Single<IPersistentProgressService>(),
                _services.Single<IUpgradeService>(),
                _services.Single<IShopService>(),
                _services.Single<IBuffService>(),
                _services.Single<ISelectedLevelService>(),
                _services.Single<ICritDamageService>(),
                _services.Single<IAudioService>(),
                _services.Single<IPerkService>(),
                _services.Single<IVfxService>()));
            _services.RegisterSingle<ISaveLoadService>( new SaveLoadService(
                _services.Single<IPersistentProgressService>(),
                _services.Single<IGameFactory>()));
        }
        
        private static IEnumerator ApplyAudioSettingsNextFrame(ISettingsService settings)
        {
            yield return null;
            settings.ApplyAudio();
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