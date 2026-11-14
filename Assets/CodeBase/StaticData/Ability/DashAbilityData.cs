using UnityEngine;

namespace CodeBase.StaticData.Ability
{
    [CreateAssetMenu(fileName = "DashAbility", menuName = "StaticData/Ability/Dash")]
    public class DashAbilityData : AbilityStaticData
    {
        [Header("Dash Settings")]
        public float dashDistance = 5f;
        public float dashDuration = 0.2f;
        public AnimationCurve dashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Perk: Deadly Dash")]
        [Tooltip("Запас навколо колайдера тіла гравця, у якому ривок зачіпає ворогів (діє лише з перком Deadly Dash).")]
        [Min(0f)] public float dashDamageRadius = 0.5f;
    }
}