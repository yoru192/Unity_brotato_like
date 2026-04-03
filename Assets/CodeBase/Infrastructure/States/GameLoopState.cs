using CodeBase.Infrastructure.Services.ProgressService;
using CodeBase.Infrastructure.Services.ShopService;

namespace CodeBase.Infrastructure.States
{
    public class GameLoopState : IState
    {
        private readonly IGameStateMachine _stateMachine;
        private readonly IProgressService _progressService;
        private readonly IShopService _shopService;
        private bool _shopTimerStarted;

        public GameLoopState(IGameStateMachine stateMachine, IProgressService progressService, IShopService shopService)
        {
            _stateMachine = stateMachine;
            _progressService = progressService;
            _shopService = shopService;
        }

        public void Enter()
        {
            _progressService.OnLevelUp += OnLevelUp;
            _shopService.OnShopTimerTick += OnShopTimerTick;
    
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
        }

        private void OnLevelUp(int newLevel)
        {
            _stateMachine.Enter<UpgradeState>();
        }

        private void OnShopTimerTick()
        {
            _stateMachine.Enter<ShopState>();
        }
    }
}