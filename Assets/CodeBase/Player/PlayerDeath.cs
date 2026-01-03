using System;
using CodeBase.Player.Movement;
using CodeBase.Weapon;
using UnityEngine;

namespace CodeBase.Player
{
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerDeath : MonoBehaviour
    {
        public PlayerHealth health;
        public PlayerMovement movement;
        public WeaponAttack attack;
        private bool _isDeath;

        private void Start()
        {
            health.HealthChanged += HealthChanged;
        }

        private void OnDestroy()
        {
            health.HealthChanged -= HealthChanged;
        }

        private void HealthChanged()
        {
            if (!_isDeath && health.Current <= 0)
                Die();
        }

        private void Die()
        {
            _isDeath = true;
            movement.enabled = false;
            attack.enabled = false;
            
        }
    }
}