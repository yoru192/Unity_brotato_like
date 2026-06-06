using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.Map;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.ProgressService;
using CodeBase.Infrastructure.Services.SelectedLevel;

namespace CodeBase.Infrastructure.States
{
    public class GameLoopState : IState
    {
        // Small heal granted after clearing a combat node so HP slowly recovers along the map.
        private const float HealAfterRun = 6f;

        private readonly IGameStateMachine _stateMachine;
        private readonly IProgressService _progressService;
        private readonly IGameFactory _gameFactory;
        private readonly IMapService _mapService;
        private readonly ISelectedLevelService _selectedLevel;
        private readonly IPersistentProgressService _persistentProgress;

        public GameLoopState(IGameStateMachine stateMachine, IProgressService progressService,
            IGameFactory gameFactory, IMapService mapService, ISelectedLevelService selectedLevel,
            IPersistentProgressService persistentProgress)
        {
            _stateMachine = stateMachine;
            _progressService = progressService;
            _gameFactory = gameFactory;
            _mapService = mapService;
            _selectedLevel = selectedLevel;
            _persistentProgress = persistentProgress;
        }

        public void Enter()
        {
            _progressService.OnLevelUp += OnLevelUp;

            if (_gameFactory.WaveController != null)
                _gameFactory.WaveController.OnRunCompleted += OnRunCompleted;

            if (_gameFactory.PauseInputHandler != null)
                _gameFactory.PauseInputHandler.OnPausePressed += OnPausePressed;
        }

        public void Exit()
        {
            _progressService.OnLevelUp -= OnLevelUp;

            if (_gameFactory.WaveController != null)
                _gameFactory.WaveController.OnRunCompleted -= OnRunCompleted;

            if (_gameFactory.PauseInputHandler != null)
                _gameFactory.PauseInputHandler.OnPausePressed -= OnPausePressed;
        }

        private void OnLevelUp(int newLevel)
        {
            _stateMachine.Enter<UpgradeState>();
        }

        private void OnRunCompleted()
        {
            // Launched outside the map flow (e.g. direct editor play): keep the old behaviour.
            if (_selectedLevel.SelectedNode == null)
            {
                _stateMachine.Enter<WinState>();
                return;
            }

            bool wasBoss = _selectedLevel.SelectedNode.IsBoss;

            // Passive recovery between nodes.
            _persistentProgress.Progress.playerState.Heal(HealAfterRun);

            _mapService.CompleteSelectedNode();

            if (wasBoss || _mapService.IsMapCompleted)
                _stateMachine.Enter<WinState>();
            else
                _stateMachine.Enter<LevelMapState>();
        }

        private void OnPausePressed()
        {
            _stateMachine.Enter<PauseState>();
        }
    }
}