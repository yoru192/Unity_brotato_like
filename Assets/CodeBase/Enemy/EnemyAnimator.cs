using UnityEngine;

namespace CodeBase.Enemy
{
    public class EnemyAnimator : MonoBehaviour
    {
        private Animator _animator;
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
        
        public void PlayAttack() => _animator.SetTrigger(AttackHash);
    }
}