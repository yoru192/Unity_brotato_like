using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.Perk;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.StaticData;
using CodeBase.StaticData.Perk;
using CodeBase.StaticData.Weapon;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CodeBase.Infrastructure.Services.Upgrade
{
       public class UpgradeService : IUpgradeService
    {
        public event Action OnUpgradeStamina;
        public event Action OnUpgradeWeapon;
        public event Action OnUpgradeHP;
        public event Action OnUpgradeMoveSpeed;
        private IStaticDataService _staticData;
        private readonly IPersistentProgressService _progressService;
        private readonly IPerkService _perkService;

        public UpgradeService(IStaticDataService staticDataService, IPersistentProgressService progressService,
            IPerkService perkService)
        {
            _staticData =  staticDataService;
            _progressService = progressService;
            _perkService = perkService;
        }
        private PlayerProgress Progress => _progressService.Progress;
        private State PlayerState => Progress.playerState;
        
        
        
        public List<UpgradeStaticData> GenerateUpgradeOptions(int count = 4)
        {
            // Лише ті апгрейди, що зараз мають сенс (напр. перк для лука не пропонуємо без лука).
            List<UpgradeStaticData> allUpgrades = new List<UpgradeStaticData>();
            foreach (UpgradeStaticData candidate in _staticData.GetAllUpgrades())
            {
                if (IsAvailable(candidate))
                    allUpgrades.Add(candidate);
            }

            List<UpgradeStaticData> remaining = new List<UpgradeStaticData>(allUpgrades);

            List<UpgradeStaticData> selected = new List<UpgradeStaticData>();
            for (int i = 0; i < count; i++)
            {
                // Prefer distinct options; once the unique pool runs out, fall back to repeats
                // so we still fill all slots when there are fewer upgrade definitions than count.
                List<UpgradeStaticData> source = remaining.Count > 0 ? remaining : allUpgrades;
                if (source.Count == 0)
                    break;

                UpgradeStaticData upgrade = GetWeightedRandom(source);
                selected.Add(upgrade);
                remaining.Remove(upgrade);
            }

            return selected;
        }
        
        private bool IsAvailable(UpgradeStaticData upgrade)
        {
            if (upgrade == null) return false;
            if (!upgrade.IsPerk) return true;

            if (upgrade.maxPicks > 0 && _perkService.GetPicks(upgrade.perkId) >= upgrade.maxPicks)
                return false;

            switch (upgrade.perkId)
            {
                case PerkId.MultiShot:
                    return PlayerState.OwnedWeapons.Contains(WeaponTypeId.Arrow);
                case PerkId.WideSwing:
                case PerkId.Vampirism:
                    return PlayerState.OwnedWeapons.Contains(WeaponTypeId.Melee);
                default:
                    return true;
            }
        }

        public void ApplyUpgrade(UpgradeStaticData upgrade)
        {
            if (upgrade.IsPerk)
            {
                _perkService.Add(upgrade.perkId, upgrade.value);
                return;
            }

            switch (upgrade.modifierType)
            {
                case StatModifierType.MaxHealth:
                    PlayerState.maxHealth += upgrade.value;
                    PlayerState.currentHealth = Mathf.Min(
                        PlayerState.currentHealth + upgrade.value, 
                        PlayerState.maxHealth
                    );
                    OnUpgradeHP?.Invoke();
                    break;
                case StatModifierType.CurrentHealth:
                    PlayerState.currentHealth = Mathf.Min(
                        PlayerState.currentHealth + upgrade.value, 
                        PlayerState.maxHealth
                    );
                    OnUpgradeHP?.Invoke();
                    break;
                case StatModifierType.MaxHealthPercent:
                    float bonus = PlayerState.maxHealth * (upgrade.value / 100f);
                    PlayerState.maxHealth += bonus;
                    PlayerState.currentHealth += bonus;
                    OnUpgradeHP?.Invoke();
                    break;
                case StatModifierType.MoveSpeed:
                    PlayerState.moveSpeed += upgrade.value;
                    OnUpgradeMoveSpeed?.Invoke();
                    break;
                case StatModifierType.Damage:
                    if (PlayerState.OwnedWeapons.Contains(WeaponTypeId.Melee))
                        PlayerState.MeleeWeaponState.weaponDamage += upgrade.value;
                    if (PlayerState.OwnedWeapons.Contains(WeaponTypeId.Arrow))
                        PlayerState.ArrowWeaponState.weaponDamage += upgrade.value;
                    OnUpgradeWeapon?.Invoke();
                    break;
                case StatModifierType.Cooldown:
                    if (PlayerState.OwnedWeapons.Contains(WeaponTypeId.Melee))
                        PlayerState.MeleeWeaponState.weaponCooldown += upgrade.value;
                    if (PlayerState.OwnedWeapons.Contains(WeaponTypeId.Arrow))
                        PlayerState.ArrowWeaponState.weaponCooldown += upgrade.value;
                    OnUpgradeWeapon?.Invoke();
                    break;
                case StatModifierType.MaxStamina:
                    PlayerState.maxStamina += upgrade.value;
                    PlayerState.currentStamina += upgrade.value;
                    OnUpgradeStamina?.Invoke();
                    break;
                case StatModifierType.RegenRateStamina:
                    PlayerState.regenRateStamina += upgrade.value;
                    OnUpgradeStamina?.Invoke();
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
