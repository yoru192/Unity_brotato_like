using System;

namespace CodeBase.Infrastructure.Services.ProgressService
{
    public interface IProgressService : IService
    {
        event Action<int> OnXpGained;
        event Action<int> OnLevelUp;
        void AddXp(int amount);
        int CalculateXpForNextLevel();
    }
}