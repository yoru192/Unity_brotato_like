using System;
using System.Collections.Generic;
using CodeBase.StaticData.Perk;
using CodeBase.StaticData.Weapon;
using UnityEngine;

namespace CodeBase.Data
{
    [Serializable]
    public class State
    {
        public MeleeWeaponState MeleeWeaponState = new MeleeWeaponState();
        public ArrowWeaponState ArrowWeaponState = new ArrowWeaponState();
        public List<WeaponTypeId> OwnedWeapons = new List<WeaponTypeId>();

        /// <summary>Перки, отримані в цьому забігу. Працювати з ними через IPerkService.</summary>
        public List<PerkEntry> perks = new List<PerkEntry>();
        public float currentStamina;
        public float maxStamina;
        public float regenRateStamina;
        public int currentXp;
        public int currentLevel = 1;
        public int maxLevel = 10;
        public float currentHealth;
        public float maxHealth;
        public float moveSpeed;
        public int currentBalance = 0;
        
        public void ResetHealth() =>
            currentHealth = maxHealth;

        /// <summary>Restores health by <paramref name="amount"/>, clamped to maxHealth.</summary>
        public void Heal(float amount) =>
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

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

    [Serializable]
    public struct PerkEntry
    {
        public PerkId id;

        /// <summary>Сума value всіх узятих апгрейдів цього перку.</summary>
        public float value;

        /// <summary>Скільки разів перк було взято.</summary>
        public int picks;
    }

    // [Serializable] обов'язковий: без нього JsonUtility мовчки пропускає ці поля у збереженні.
    [Serializable]
    public class WeaponState
    {
        public float weaponDamage;
        public float weaponCooldown;
    }

    [Serializable]
    public class ArrowWeaponState : WeaponState
    {
    }

    [Serializable]
    public class MeleeWeaponState : WeaponState
    {
    }
}