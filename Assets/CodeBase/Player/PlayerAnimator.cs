using UnityEngine;

namespace CodeBase.Player
{
    public class PlayerAnimator : MonoBehaviour
    {
        private readonly int _walkingStateHash = Animator.StringToHash("Walking");
        
        private Animator _animator;
        private Rigidbody2D _rb;
        private int _facingDirection = 1;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody2D>();
        }
        
        private void Update()
        {
            _animator.SetFloat(_walkingStateHash, _rb.linearVelocity.magnitude, 0.1f, Time.deltaTime);

            if (_rb.linearVelocity.x != 0)
                _facingDirection = _rb.linearVelocity.x > 0 ? 1 : -1;
            
            transform.localScale = new Vector2(_facingDirection, 1);
        }
        
       
    }
}