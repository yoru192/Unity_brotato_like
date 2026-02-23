using UnityEngine;

namespace CodeBase.Weapon
{
    public interface IAttackColliderHandler
    {
        void OnTargetEnter(Collider2D other);
        void OnTargetExited(Collider2D other);
    }
}