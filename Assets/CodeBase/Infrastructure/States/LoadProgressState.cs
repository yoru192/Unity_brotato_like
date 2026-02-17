using CodeBase.Data;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.Logic;
using CodeBase.StaticData;

namespace CodeBase.Infrastructure.States
{
    public class LoadProgressState : IState
    {
        private readonly GameStateMachine _gameStateMachine;
        private readonly IPersistentProgressService _progressService;
        private readonly ISaveLoadService _saveLoadService;
        private readonly PlayerStaticData _playerData;

        public LoadProgressState(GameStateMachine gameStateMachine, IPersistentProgressService  progressService, ISaveLoadService saveLoadService, PlayerStaticData playerData)
        {
            _gameStateMachine =  gameStateMachine;
            _progressService = progressService;
            _saveLoadService = saveLoadService;
            _playerData = playerData;
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

            playerProgress.playerState.maxHealth = _playerData.maxHealth;
            playerProgress.playerState.ResetHealth();
            playerProgress.playerState.moveSpeed = _playerData.moveSpeed;
            
            return playerProgress;
        }
    }
}