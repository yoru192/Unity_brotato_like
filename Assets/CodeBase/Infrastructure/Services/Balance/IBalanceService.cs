using System;

namespace CodeBase.Infrastructure.Services.Balance
{
    public interface IBalanceService : IService
    {
        event Action OnBalanceChanged;
        int CurrentBalance { get; }
        void AddBalance(int amount);
        bool TryRemoveBalance(int amount);
    }
}