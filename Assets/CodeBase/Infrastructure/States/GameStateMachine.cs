using System;
using System.Collections.Generic;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.Infrastructure.Services.Upgrade;
using CodeBase.Logic;
using CodeBase.StaticData;

namespace CodeBase.Infrastructure.States
{
    public class GameStateMachine : IGameStateMachine
    {
        private readonly Dictionary<Type, IExitableState> _states;
        private IExitableState _activeState;

        public GameStateMachine(SceneLoader sceneLoader, LoadingCurtain curtain, AllServices services)
        {
            _states = new Dictionary<Type, IExitableState>
            {
                [typeof(BootstrapState)] = new BootstrapState(this, sceneLoader, services),
                [typeof(LoadLevelState)] = new LoadLevelState(this, 
                    sceneLoader,
                    curtain,
                    services.Single<IGameFactory>(),
                    services.Single<IPersistentProgressService>(),
                    services.Single<IStaticDataService>(),
                    services.Single<IProgressService>()
                ),
                [typeof(LoadProgressState)] = new LoadProgressState(
                    this, 
                    services.Single<IPersistentProgressService>(), 
                    services.Single<ISaveLoadService>()),
                [typeof(UpgradeState)] = new UpgradeState(this,services.Single<IGameFactory>(),services.Single<IUpgradeService>()),
                [typeof(GameLoopState)] = new GameLoopState(this,services.Single<IProgressService>()),
                [typeof(GameOverState)] = new GameOverState(this, services.Single<IGameFactory>()),
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

