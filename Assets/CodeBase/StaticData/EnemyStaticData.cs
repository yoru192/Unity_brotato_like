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
        [Range(1,7)]
        public int damage;
        [Range(1,10)]
        public float moveSpeed;
        
        public AssetReferenceGameObject prefabReference;
    }
}