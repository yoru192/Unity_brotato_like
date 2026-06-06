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
            // Raw stored value: 0 means dead, not "uninitialized". Initial full health is set up
            // in LoadProgress for fresh progress, so 0 here never silently reads back as max.
            get => _persistentProgressService.Progress.playerState.currentHealth;
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
            var state = progress.playerState;

            // Fresh progress (max never set) starts at full health. Otherwise keep the stored
            // values so a player who hit 0 stays dead instead of being revived to full.
            if (state.maxHealth <= 0f)
            {
                state.maxHealth = _playerData.maxHealth;
                state.currentHealth = _playerData.maxHealth;
            }

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