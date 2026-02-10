using System;
using UnityEngine;

namespace CodeBase.Data
{
    [Serializable]
    public class State
    {
        public float currentStamina = 100f;
        public float maxStamina = 100f;
        public float regenRateStamina = 15f;
        public int currentXp;
        public int currentLevel = 1;
        public int maxLevel = 10;
        public float currentHealth;
        public float maxHealth;
        public float moveSpeed;
        public float weaponDamage;
        public float weaponCooldown;
        
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