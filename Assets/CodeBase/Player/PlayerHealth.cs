using System;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Player
{
    public class PlayerHealth : MonoBehaviour, ISavedProgress, IHealth
    {
        public event Action HealthChanged;
        
        private State _state;

        public float Current
        {
            get => _state?.currentHealth ?? 0f;
            set
            {
                if(_state.currentHealth != value)
                {
                    _state.currentHealth = value;
                    HealthChanged?.Invoke();
                }
            }
        }

        public float Max
        {
            get => _state?.maxHealth ?? 0f;
            set => _state.maxHealth = value;
        }

        public void LoadProgress(PlayerProgress progress)
        {
            _state = progress.playerState;
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
            Debug.Log($"Current HP: {Current}");
        }
    }
}