using System;

namespace CodeBase.Infrastructure.Services.Balance
{
    public interface IBalanceService : IService
    {
        event Action <int> OnBalanceGained;
        void AddBalance(int amount);
    }
}