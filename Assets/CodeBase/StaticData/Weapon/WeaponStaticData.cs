using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CodeBase.StaticData.Weapon
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "StaticData/Weapon")]
    public class WeaponStaticData : ScriptableObject
    {
        public WeaponTypeId weaponTypeId;
        [Range(1,30)]
        public int damage;
        [Range(0.1f, 100f)]
        public float radius;
        public AssetReferenceGameObject prefabReference;
        
        [Header("Ranged Only")]
        public float projectileSpeed = 8f;
        public float shootRate = 1f;
        [Header("Melee Only")]
        [Range(10f, 360f)]
        public float attackAngle;
        public float cooldown;
    }
}