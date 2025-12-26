using System;

namespace CodeBase.Data
{
    [Serializable]
    public class State
    {
        public float currentHealth;
        public float maxHealth;
        
        public void ResetHealth() =>
            currentHealth = maxHealth;
    }
}