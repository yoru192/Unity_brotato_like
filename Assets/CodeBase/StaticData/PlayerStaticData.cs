using UnityEngine;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "StaticData/Player")]
    public class PlayerStaticData : ScriptableObject
    {
        public float maxStamina;
        public float regenRateStamina;
        public float maxHealth;
        public float moveSpeed;
    }
}