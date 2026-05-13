using System;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Logic;
using CodeBase.StaticData;
using CodeBase.StaticData.Hero;
using UnityEngine;

namespace CodeBase.Player
{
    public class PlayerHealth : MonoBehaviour, ISavedProgress, IHealth
    {
        public event Action HealthChanged;
        private HeroStaticData _playerData;
        private IPersistentProgressService _persistentProgressService;

        public float Current
        {
            get => _persistentProgressService.Progress.playerState.currentHealth == 0 ? _playerData.maxHealth : _persistentProgressService.Progress.playerState.currentHealth;
            set
            {
                if(!Mathf.Approximately(_persistentProgressService.Progress.playerState.currentHealth, value))
                {
                    _persistentProgressService.Progress.playerState.currentHealth = value;
                    HealthChanged?.Invoke();
                }
            }
        }
        public float Max
        {
            get => _persistentProgressService.Progress.playerState.maxHealth == 0 ? _playerData.maxHealth : _persistentProgressService.Progress.playerState.maxHealth;
            set => _persistentProgressService.Progress.playerState.maxHealth = value;
        }
        public void Construct(HeroStaticData playerData, IPersistentProgressService persistentProgressService)
        {
            _playerData = playerData;
            _persistentProgressService = persistentProgressService;
        }

        public void LoadProgress(PlayerProgress progress)
        {
            Current = progress.playerState.currentHealth;
            Max = progress.playerState.maxHealth == 0 ? _playerData.maxHealth : progress.playerState.maxHealth;
            HealthChanged?.Invoke();
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            progress.playerState.currentHealth = Current;
            progress.playerState.maxHealth = Max;
        }
        
        

        public void TakeDamage(float damage)
        {
            if(Current <= 0)
                return; 
            
            Current -= damage;
            HealthChanged?.Invoke();
        }
        
    }
}