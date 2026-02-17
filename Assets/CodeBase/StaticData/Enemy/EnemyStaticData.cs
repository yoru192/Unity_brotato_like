using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CodeBase.StaticData.Enemy
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
        [Range(0.1f, 10)] 
        public float cooldown;
        [Range(.5f,10f)]
        public float radius = .5f;
        [Range(1, 50)] 
        public int xpReward;
        [Range(1, 50)] 
        public int balanceReward;
        public AssetReferenceGameObject prefabReference;
        public Vector3 spriteScale;
    }
}