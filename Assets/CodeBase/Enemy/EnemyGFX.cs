using Pathfinding;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class EnemyGFX : MonoBehaviour
    {
        public FollowerEntity followerEntity;
        private Vector3 _spriteScale;

        public void Construct(Vector3 spriteScale)
        {
            _spriteScale = spriteScale;
        }
    
        void Start()
        {
            if (followerEntity == null)
            {
                followerEntity = GetComponent<FollowerEntity>();
            }
        }

        void Update()
        {
            if (followerEntity.velocity.x >= 0.01f)
            {
                transform.localScale = _spriteScale;
            }
            else if (followerEntity.velocity.x <= -0.01f)
            {
                transform.localScale = new Vector3(-_spriteScale.x, _spriteScale.y, _spriteScale.z);
            }
        }
    }
}