using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.ProgressService;
using CodeBase.Infrastructure.Services.ShopService;

namespace CodeBase.Infrastructure.States
{
    public class GameLoopState : IState
    {
        private readonly IGameStateMachine _stateMachine;
        private readonly IProgressService _progressService;
        private readonly IShopService _shopService;
        private readonly IGameFactory _gameFactory;
        private bool _shopTimerStarted;

        public GameLoopState(IGameStateMachine stateMachine, IProgressService progressService, IShopService shopService, IGameFactory gameFactory)
        {
            _stateMachine = stateMachine;
            _progressService = progressService;
            _shopService = shopService;
            _gameFactory = gameFactory;
        }

        public void Enter()
        {
            _progressService.OnLevelUp += OnLevelUp;
            _shopService.OnShopTimerTick += OnShopTimerTick;

            if (_gameFactory.WaveController != null)
                _gameFactory.WaveController.OnRunCompleted += OnRunCompleted;

            if (!_shopTimerStarted)
            {
                _shopService.StartShopTimer();
                _shopTimerStarted = true;
            }
        }

        public void Exit()
        {
            _progressService.OnLevelUp -= OnLevelUp;
            _shopService.OnShopTimerTick -= OnShopTimerTick;

            if (_gameFactory.WaveController != null)
                _gameFactory.WaveController.OnRunCompleted -= OnRunCompleted;
        }

        private void OnLevelUp(int newLevel)
        {
            _stateMachine.Enter<UpgradeState>();
        }

        private void OnShopTimerTick()
        {
            _stateMachine.Enter<ShopState>();
        }

        private void OnRunCompleted()
        {
            _stateMachine.Enter<WinState>();
        }
    }
}