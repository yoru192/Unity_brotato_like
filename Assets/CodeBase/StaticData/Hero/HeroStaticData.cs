using System.Collections.Generic;
using CodeBase.StaticData.Weapon;
using UnityEngine;

namespace CodeBase.StaticData.Hero
{
    
    [CreateAssetMenu(menuName = "StaticData/Hero")]
    public class HeroStaticData : ScriptableObject
    {
        public HeroTypeId heroTypeId;
        [Header("Only UI")]
        public string HeroName;
        public string HeroClass;
        [TextArea] public string Description;
        public Sprite HeroImage;
        public Sprite Icon;
        
        [Header("Only Metadata")]
        public float maxStamina;
        public float regenRateStamina;
        public float maxHealth;
        public float moveSpeed;
        public List<WeaponTypeId> startWeapons = new() { WeaponTypeId.Melee };

    }
}