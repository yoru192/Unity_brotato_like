using System;
using CodeBase.Infrastructure.Services.PersistentProgress;

namespace CodeBase.Infrastructure.Services.Balance
{
    public class BalanceService : IBalanceService
    {
        private readonly IPersistentProgressService _persistentProgressService;
        
        public int CurrentBalance => 
            _persistentProgressService.Progress.playerState.currentBalance;
        public event Action OnBalanceChanged;

        public BalanceService(IPersistentProgressService  persistentProgressService)
        {
            _persistentProgressService = persistentProgressService;
        }

        public void AddBalance(int amount)
        {
            _persistentProgressService.Progress.playerState.currentBalance += amount;
            OnBalanceChanged?.Invoke();
        }

        public bool TryRemoveBalance(int amount)
        {
            if (_persistentProgressService.Progress.playerState.currentBalance >= amount)
            {
                _persistentProgressService.Progress.playerState.currentBalance -= amount;
                OnBalanceChanged?.Invoke();
                return true;
            }
            return false;
        }
    }
}