using UnityEngine;

namespace CodeBase.Weapon
{
    public class AttackColliderTrigger : MonoBehaviour
    {
        private WeaponAttack _weaponAttack;
        private int _layerMask;
        
        public void Initialize(WeaponAttack weaponAttack, int layerMask)
        {
            _weaponAttack = weaponAttack;
            _layerMask = layerMask;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_weaponAttack != null)
            {
                // Перевірити шар
                if (((1 << other.gameObject.layer) & _layerMask) != 0)
                {
                    _weaponAttack.OnTargetEntered(other);
                }
            }
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (_weaponAttack != null)
            {
                _weaponAttack.OnTargetExited(other);
            }
        }
    }
}