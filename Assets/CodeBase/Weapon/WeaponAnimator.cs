using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using UnityEngine;

namespace CodeBase.Weapon
{
    public class WeaponAnimator : MonoBehaviour
    {
        private Animator _animator;
        private AnimationEvents _animationEvents;
        private static readonly int AttackHash = Animator.StringToHash("Slash");
        
        public AnimationEvents AnimationEvents => _animationEvents;
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _animationEvents = GetComponent<AnimationEvents>();
            
            if (_animationEvents == null)
            {
                Debug.LogWarning($"AnimationEvents component not found on {gameObject.name}");
            }
        }
        
        public void PlayAttack() => _animator.SetTrigger(AttackHash);
    }
}