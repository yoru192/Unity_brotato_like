using System;
using CodeBase.Infrastructure.Services.PersistentProgress;

namespace CodeBase.Infrastructure.Services.Balance
{
    public class BalanceService : IBalanceService
    {
        private readonly IPersistentProgressService _persistentProgressService;
        
        public event Action <int> OnBalanceGained;

        public BalanceService(IPersistentProgressService  persistentProgressService)
        {
            _persistentProgressService = persistentProgressService;
        }

        public void AddBalance(int amount)
        {
            _persistentProgressService.Progress.playerState.currentBalance += amount;
            OnBalanceGained?.Invoke(amount);
        }
        
    }
}