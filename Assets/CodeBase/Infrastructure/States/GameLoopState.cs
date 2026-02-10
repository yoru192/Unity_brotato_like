using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.ProgressService;
using UnityEngine;

namespace CodeBase.Infrastructure.States
{
    public class GameLoopState : IState
    {
        private readonly IGameStateMachine _stateMachine;
        private readonly IProgressService _progressService;

        public GameLoopState(IGameStateMachine stateMachine, IProgressService progressService)
        {
            _stateMachine = stateMachine;
            _progressService = progressService;
        }

        public void Enter()
        {
            _progressService.OnLevelUp += OnLevelUp;
        }

        public void Exit()
        {
            _progressService.OnLevelUp -= OnLevelUp;
        }

        private void OnLevelUp(int newLevel)
        {
            _stateMachine.Enter<UpgradeState>();
        }
    }
}