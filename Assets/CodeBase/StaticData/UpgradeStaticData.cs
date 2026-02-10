using CodeBase.Data;
using UnityEngine;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(fileName = "Upgrade", menuName = "StaticData/Upgrade")]
    public class UpgradeStaticData : ScriptableObject
    {
        [Header("UI Info")]
        public string upgradeName;
        [TextArea] public string description;
        public Sprite icon;
    
        [Header("Effect")]
        public StatModifierType modifierType;
        public float value;
    
        [Header("Optional")]
        public int weight = 1;
    }

    public enum StatModifierType
    {
        MaxHealth,
        CurrentHealth,
        MaxHealthPercent,
        MoveSpeed,
        Damage,
        Cooldown,
        MaxStamina,
        RegenRateStamina,
    }

}
