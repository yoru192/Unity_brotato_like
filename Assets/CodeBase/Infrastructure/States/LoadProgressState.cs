using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.StaticData;
using CodeBase.StaticData.Hero;
using UnityEngine;

namespace CodeBase.Infrastructure.States
{
    public class LoadProgressState : IState
    {
        private readonly GameStateMachine _gameStateMachine;
        private readonly IPersistentProgressService _progressService;
        private readonly ISaveLoadService _saveLoadService;
        private readonly HeroStaticData _heroData;

        public LoadProgressState(GameStateMachine gameStateMachine,
            IPersistentProgressService  progressService,
            ISaveLoadService saveLoadService
            )
        {
            _gameStateMachine =  gameStateMachine;
            _progressService = progressService;
            _saveLoadService = saveLoadService;
        }
        public void Enter()
        {
            Debug.Log("Entering LoadProgressState");
            LoadProgressOrInitNew();
            _gameStateMachine.Enter<LoadoutSelectState>();
        }

        public void Exit()
        {

        }
        
        private void LoadProgressOrInitNew()
        {
            _progressService.Progress = 
                _saveLoadService.LoadProgress() 
                ?? NewProgress();
        }
        
        private PlayerProgress NewProgress()
        {
            return new PlayerProgress("Main");
        }
    }
}