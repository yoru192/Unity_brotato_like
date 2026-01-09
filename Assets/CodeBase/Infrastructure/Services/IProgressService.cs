using System;
using CodeBase.Data;

namespace CodeBase.Infrastructure.Services
{
    public interface IProgressService : IService
    {
        event Action<int> OnXpGained;
        event Action<int> OnLevelUp;
        void AddXp(int amount);
        int CalculateXpForNextLevel();
    }
}