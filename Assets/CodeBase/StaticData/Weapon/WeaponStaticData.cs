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
        [Range(0, 100)]
        public int critChance;
        [Range(1, 5)]
        public float critRate;
        public AssetReferenceGameObject prefabReference;
        
        [ShowIf("weaponTypeId", WeaponTypeId.Arrow)]
        public float projectileSpeed = 8f;
        
        [ShowIf("weaponTypeId", WeaponTypeId.Arrow)]
        public float shootRate = 1f;
        
        [Range(10f, 360f)]
        [ShowIf("weaponTypeId", WeaponTypeId.Melee)]
        public float attackAngle;
        
        [ShowIf("weaponTypeId", WeaponTypeId.Melee)]
        public float cooldown;
    }
}