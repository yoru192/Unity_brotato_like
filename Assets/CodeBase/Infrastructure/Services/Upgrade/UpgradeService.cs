using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.StaticData;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CodeBase.Infrastructure.Services.Upgrade
{
       public class UpgradeService : IUpgradeService
    {
        private IStaticDataService _staticData;
        private readonly IPersistentProgressService _progressService;

        public UpgradeService(IStaticDataService staticDataService, IPersistentProgressService progressService)
        {
            _staticData =  staticDataService;
            _progressService = progressService;
        }
        private PlayerProgress Progress => _progressService.Progress;
        private State PlayerState => Progress.playerState;
        
        
        
        public List<UpgradeStaticData> GenerateUpgradeOptions(int count = 3)
        {
            List<UpgradeStaticData> allUpgrades = _staticData.GetAllUpgrades();
            
            List<UpgradeStaticData> selected = new List<UpgradeStaticData>();
            for (int i = 0; i < count; i++)
            {
                UpgradeStaticData upgrade = GetWeightedRandom(allUpgrades);
                selected.Add(upgrade);
            }
            
            return selected;
        }
        
        public void ApplyUpgrade(UpgradeStaticData upgrade)
        {
            switch (upgrade.modifierType)
            {
                case StatModifierType.MaxHealth:
                    PlayerState.maxHealth += upgrade.value;
                    PlayerState.currentHealth += upgrade.value;
                    break;
            
                case StatModifierType.CurrentHealth:
                    PlayerState.currentHealth = Mathf.Min(
                        PlayerState.currentHealth + upgrade.value, 
                        PlayerState.maxHealth
                    );
                    break;
            
                case StatModifierType.MaxHealthPercent:
                    float bonus = PlayerState.maxHealth * (upgrade.value / 100f);
                    PlayerState.maxHealth += bonus;
                    PlayerState.currentHealth += bonus;
                    break;
            
                case StatModifierType.MoveSpeed:
                    PlayerState.moveSpeed += upgrade.value;
                    break;
                
                case StatModifierType.Damage:
                    PlayerState.weaponDamage += upgrade.value;
                    break;
            
                case StatModifierType.Cooldown:
                    PlayerState.weaponCooldown += upgrade.value;
                    break;
                case StatModifierType.MaxStamina:
                    PlayerState.maxStamina += upgrade.value;
                    break;
                case StatModifierType.RegenRateStamina:
                    PlayerState.regenRateStamina += upgrade.value;
                    break;
            }
            
            if (!Progress.AppliedUpgrades.ContainsKey(upgrade.modifierType))
                Progress.AppliedUpgrades[upgrade.modifierType] = 0;
    
            Progress.AppliedUpgrades[upgrade.modifierType] += upgrade.value;
        }

        
        private UpgradeStaticData GetWeightedRandom(List<UpgradeStaticData> upgrades)
        {
            int totalWeight = upgrades.Sum(u => u.weight);
            int randomValue = Random.Range(0, totalWeight);
            
            int currentWeight = 0;
            foreach (var upgrade in upgrades)
            {
                currentWeight += upgrade.weight;
                if (randomValue < currentWeight)
                    return upgrade;
            }
            
            return upgrades[0];
        }
    }

}
