using UnityEngine;

namespace CodeBase.StaticData.Ability
{
    [CreateAssetMenu(fileName = "AbilityData", menuName = "StaticData/Ability")]
    public class AbilityStaticData : ScriptableObject
    {
        public AbilityTypeId abilityTypeId;
        public Sprite abilityIcon;
        [Range(1f, 10f)]
        public float abilityCooldown;
        public string menuName;
    }
}