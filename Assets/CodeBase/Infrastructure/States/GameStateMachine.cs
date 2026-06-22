using System;
using System.Collections.Generic;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Audio;
using CodeBase.Infrastructure.Services.Balance;
using CodeBase.Infrastructure.Services.Map;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.ProgressService;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.Infrastructure.Services.SelectedLevel;
using CodeBase.Infrastructure.Services.ShopService;
using CodeBase.Infrastructure.Services.Upgrade;
using CodeBase.Logic;
using CodeBase.Player;
using CodeBase.StaticData;

namespace CodeBase.Infrastructure.States
{
    public class GameStateMachine : IGameStateMachine
    {
        private readonly Dictionary<Type, IExitableState> _states;
        private IExitableState _activeState;

        public GameStateMachine(SceneLoader sceneLoader, LoadingCurtain curtain, AllServices services,
            ICoroutineRunner coroutineRunner)
        {
            _states = new Dictionary<Type, IExitableState>
            {
                [typeof(BootstrapState)] = new BootstrapState(this, sceneLoader, services, coroutineRunner),
                [typeof(LoadoutSelectState)] = new LoadoutSelectState(sceneLoader),
                [typeof(LevelMapState)] = new LevelMapState(this,
                    sceneLoader,
                    services.Single<IMapService>()),
                [typeof(LoadLevelState)] = new LoadLevelState(this,
                    sceneLoader,
                    curtain,
                    services.Single<IGameFactory>(),
                    services.Single<IPersistentProgressService>(),
                    services.Single<IStaticDataService>()),
                [typeof(LoadProgressState)] = new LoadProgressState(
                    this, 
                    services.Single<IPersistentProgressService>(), 
                    services.Single<ISaveLoadService>()),
                [typeof(ShopState)] = new ShopState(services.Single<IShopService>(), services.Single<IGameFactory>(),this, services.Single<IBalanceService>(), services.Single<IMapService>()),
                [typeof(UpgradeState)] = new UpgradeState(this,services.Single<IGameFactory>(),services.Single<IUpgradeService>(),services.Single<IBalanceService>()),
                [typeof(CampfireState)] = new CampfireState(this, services.Single<IGameFactory>(), services.Single<IMapService>(), services.Single<IUpgradeService>(), services.Single<IPersistentProgressService>(), services.Single<IAudioService>()),
                [typeof(GameLoopState)] = new GameLoopState(this, services.Single<IProgressService>(), services.Single<IGameFactory>(), services.Single<IMapService>(), services.Single<ISelectedLevelService>(), services.Single<IPersistentProgressService>()),
                [typeof(GameOverState)] = new GameOverState(this, services.Single<IGameFactory>(), services.Single<IShopService>()),
                [typeof(WinState)] = new WinState(this, services.Single<IGameFactory>(), services.Single<IShopService>()),
                [typeof(PauseState)] = new PauseState(this, services.Single<IGameFactory>(), services.Single<IShopService>()),
            };
        }
        public void Enter<TState>() where TState : class, IState
        {
            IState state = ChangeState<TState>();
            state.Enter();
        }


        public void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadedState<TPayload>
        {
            TState state = ChangeState<TState>();
            state.Enter(payload);
        }

        private TState ChangeState<TState>() where TState : class, IExitableState
        {
            _activeState?.Exit();
            TState state = GetState<TState>();
            _activeState = state;
            return state;
        }

        private TState GetState<TState>() where TState : class, IExitableState => 
            _states[typeof(TState)] as TState;
    }
}

