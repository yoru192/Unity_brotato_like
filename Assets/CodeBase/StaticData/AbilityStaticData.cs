using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(fileName = "AbilityData", menuName = "StaticData/Ability")]
    public class AbilityStaticData : ScriptableObject
    {
        public AbilityTypeId abilityTypeId;
        [Range(1f, 10f)]
        public float abilityCooldown;
        public string menuName;
    }
}