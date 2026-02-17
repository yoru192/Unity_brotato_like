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
        public float cooldown;
        [Range(0.1f, 2f)]
        public float radius = 0.5f;
        public AssetReferenceGameObject prefabReference;
    }
}