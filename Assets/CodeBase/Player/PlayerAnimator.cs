using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using UnityEngine;

namespace CodeBase.Player
{
    public class PlayerAnimator : MonoBehaviour
    {
        private Character _character;
        private Animator _animator;

        private void Awake()
        {
            _character = GetComponent<Character>();
            _animator = GetComponentInChildren<Animator>();
            
            if (_animator != null)
            {
                _animator.SetBool("Ready", true);
            }
        }
        public void PlayDeath()
        {
            if (_character != null)
            {
                _character.SetState(CharacterState.DeathB);
            }
        }
    }
}