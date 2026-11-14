using CodeBase.Data;
using CodeBase.StaticData.Perk;
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
        public Sprite background;
    
        [Header("Effect")]
        public StatModifierType modifierType;
        [Tooltip("Для Stat — скільки додати до характеристики. Для Perk — значення перку за один вибір (див. PerkId).")]
        public float value;

        [Header("Perk (лише коли kind = Perk; modifierType тоді ігнорується)")]
        public UpgradeKind kind = UpgradeKind.Stat;
        public PerkId perkId = PerkId.None;
        [Tooltip("Скільки разів перк можна взяти за забіг. 0 — без обмеження.")]
        [Min(0)] public int maxPicks;

        [Header("Optional")]
        public int weight = 1;

        public bool IsPerk => kind == UpgradeKind.Perk && perkId != PerkId.None;

    }

    public enum UpgradeKind
    {
        /// <summary>Додає value до характеристики modifierType.</summary>
        Stat = 0,

        /// <summary>Дає перк perkId (ефект, що змінює поведінку).</summary>
        Perk = 1,
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
