using System;
using System.Collections;
using UnityEngine;

namespace CodeBase.Enemy
{
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyDeath : MonoBehaviour
    {
        public EnemyHealth health;
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
            if (health.Current <= 0)
                Die();
        }

        private void Die()
        {
            health.HealthChanged -= HealthChanged;
            Destroy(gameObject);
        }
        
    }
}