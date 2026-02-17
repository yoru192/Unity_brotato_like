using System;
using UnityEngine;

namespace CodeBase.Data
{
    [Serializable]
    public class State
    {
        public float currentStamina;
        public float maxStamina;
        public float regenRateStamina;
        public int currentXp;
        public int currentLevel = 1;
        public int maxLevel = 10;
        public float currentHealth;
        public float maxHealth;
        public float moveSpeed;
        public float weaponDamage;
        public float weaponCooldown;
        public int currentBalance = 0;
        
        public void ResetHealth() =>
            currentHealth = maxHealth;

        public void ResetXp() => 
            currentXp = 0;
        public void LevelUp()
        {
            if (currentLevel < maxLevel)
            {
                currentLevel++;
                Debug.Log($"Level up! currentLevel: {currentLevel}");
                ResetXp();
            }
        }
    }
}