using UnityEngine;

namespace CodeBase.Weapon
{
    public class AttackColliderTrigger : MonoBehaviour
    {
        private IAttackColliderHandler _handler;
        private int _layerMask;

        public void Initialize(IAttackColliderHandler handler, int layerMask)
        {
            _handler = handler;
            _layerMask = layerMask;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_handler == null) return;
            if (((1 << other.gameObject.layer) & _layerMask) != 0)
                _handler.OnTargetEnter(other);
        }

        private void OnTriggerExit2D(Collider2D other) =>
            _handler?.OnTargetExited(other);
    }
}