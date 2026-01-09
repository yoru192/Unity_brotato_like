using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "StaticData/Enemy")]
    public class EnemyStaticData : ScriptableObject
    {
        public EnemyTypeId enemyTypeId;
        [Range(1, 30)] 
        public int health;
        [Range(0,10)]
        public int damage;
        [Range(1,10)]
        public float moveSpeed;
        [Range(0, 3)] 
        public float cooldown;
        [Range(.5f,3f)]
        public float effectiveDistance = .5f;
        [Range(1, 50)] 
        public int xpReward;
        public AssetReferenceGameObject prefabReference;
    }
}