using System;
using CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.ProgressService
{
    public class ProgressService : IProgressService
    {
        private readonly IPersistentProgressService _persistentProgressService;

        public ProgressService(IPersistentProgressService persistentProgressService)
        {
            _persistentProgressService = persistentProgressService;
        }
        
        public event Action<int> OnXpGained;
        public event Action<int> OnLevelUp;
    
        public void AddXp(int amount)
        {
            _persistentProgressService.Progress.playerState.currentXp += amount;
            OnXpGained?.Invoke(amount);
        
            CheckLevelUp();
            Debug.Log($"Current Xp: {_persistentProgressService.Progress.playerState.currentXp}");
        }

        private const int BaseXp = 50;
        private const float XpGrowthFactor = 1.3f;

        public int CalculateXpForNextLevel()
        {
            int level = _persistentProgressService.Progress.playerState.currentLevel;
            return Mathf.RoundToInt(BaseXp * Mathf.Pow(XpGrowthFactor, level - 1));
        }

        private void CheckLevelUp()
        {
            int requiredXp = CalculateXpForNextLevel();
    
            while (_persistentProgressService.Progress.playerState.currentXp >= requiredXp)
            {
                _persistentProgressService.Progress.playerState.currentLevel++;

                _persistentProgressService.Progress.playerState.currentXp -= requiredXp;
                
                OnLevelUp?.Invoke(_persistentProgressService.Progress.playerState.currentLevel);
                
                requiredXp = CalculateXpForNextLevel();
        
                Debug.Log($"Level up! New level: {_persistentProgressService.Progress.playerState.currentLevel}, Remaining XP: {_persistentProgressService.Progress.playerState.currentXp}");
            }
        }

    }

}