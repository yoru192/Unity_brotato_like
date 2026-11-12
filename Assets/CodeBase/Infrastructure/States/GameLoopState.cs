using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.Audio;
using CodeBase.Infrastructure.Services.Map;
using CodeBase.Infrastructure.Services.Music;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.ProgressService;
using CodeBase.Infrastructure.Services.SelectedLevel;
using CodeBase.Player.Movement;

namespace CodeBase.Infrastructure.States
{
    public class GameLoopState : IState
    {
        private const float HealAfterRun = 6f;

        private readonly IGameStateMachine _stateMachine;
        private readonly IProgressService _progressService;
        private readonly IGameFactory _gameFactory;
        private readonly IMapService _mapService;
        private readonly ISelectedLevelService _selectedLevel;
        private readonly IPersistentProgressService _persistentProgress;
        private readonly IAudioService _audioService;
        private readonly IMusicService _music;

        public GameLoopState(IGameStateMachine stateMachine, IProgressService progressService,
            IGameFactory gameFactory, IMapService mapService, ISelectedLevelService selectedLevel,
            IPersistentProgressService persistentProgress, IAudioService audioService, IMusicService musicService)
        {
            _stateMachine = stateMachine;
            _progressService = progressService;
            _gameFactory = gameFactory;
            _mapService = mapService;
            _selectedLevel = selectedLevel;
            _persistentProgress = persistentProgress;
            _audioService = audioService;
            _music = musicService;
        }

        public void Enter()
        {
            _music.Play(_selectedLevel.SelectedNode != null && _selectedLevel.SelectedNode.IsBoss
                ? MusicTrackId.Boss
                : MusicTrackId.Battle);
            _progressService.OnLevelUp += OnLevelUp;

            if (_gameFactory.WaveController != null)
                _gameFactory.WaveController.OnRunCompleted += OnRunCompleted;

            if (_gameFactory.PauseInputHandler != null)
                _gameFactory.PauseInputHandler.OnPausePressed += OnPausePressed;

            SetPlayerMovementEnabled(true);
        }

        public void Exit()
        {
            _progressService.OnLevelUp -= OnLevelUp;

            if (_gameFactory.WaveController != null)
                _gameFactory.WaveController.OnRunCompleted -= OnRunCompleted;

            if (_gameFactory.PauseInputHandler != null)
                _gameFactory.PauseInputHandler.OnPausePressed -= OnPausePressed;

            SetPlayerMovementEnabled(false);


            _audioService.StopLoop(AudioClipId.Walk);
            _audioService.StopLoop(AudioClipId.Run);
        }

        private void SetPlayerMovementEnabled(bool isEnabled)
        {
            if (_gameFactory.PlayerGameObject == null) return;

            if (_gameFactory.PlayerGameObject.TryGetComponent(out PlayerMovement playerMovement))
                playerMovement.enabled = isEnabled;
        }

        private void OnLevelUp(int newLevel)
        {
            _stateMachine.Enter<UpgradeState>();
        }

        private void OnRunCompleted()
        {
            if (_selectedLevel.SelectedNode == null)
            {
                _stateMachine.Enter<WinState>();
                return;
            }

            bool wasBoss = _selectedLevel.SelectedNode.IsBoss;
            
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