using CodeBase.Data;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.Logic;

namespace CodeBase.Infrastructure.States
{
    public class LoadProgressState : IState
    {
        private readonly GameStateMachine _gameStateMachine;
        private readonly IPersistentProgressService _progressService;
        private readonly ISaveLoadService _saveLoadService;

        public LoadProgressState(GameStateMachine gameStateMachine, IPersistentProgressService  progressService, ISaveLoadService saveLoadService)
        {
            _gameStateMachine =  gameStateMachine;
            _progressService = progressService;
            _saveLoadService = saveLoadService;
        }
        public void Enter()
        {
            LoadProgressOrInitNew();
            _gameStateMachine.Enter<LoadLevelState, string>(_progressService.Progress.worldData.positionOnLevel.Level);
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
            PlayerProgress playerProgress = new PlayerProgress("Main");

            playerProgress.playerState.maxHealth = 50f;
            playerProgress.playerState.ResetHealth();
            
            return playerProgress;
        }
    }
}