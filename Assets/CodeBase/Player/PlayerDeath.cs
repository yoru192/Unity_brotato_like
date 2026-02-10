using CodeBase.Infrastructure.States;
using CodeBase.Player.Movement;
using UnityEngine;

namespace CodeBase.Player
{
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerDeath : MonoBehaviour
    {
        [SerializeField] private PlayerHealth health;
        
        private bool _isDeath;
        private IGameStateMachine _stateMachine;
        private PlayerAnimator _animator;
        private PlayerMovement _movement;

        public void Construct(IGameStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        private void Awake()
        {
            _animator = GetComponent<PlayerAnimator>();
            _movement = GetComponent<PlayerMovement>();
        }

        private void Start()
        {
            health.HealthChanged += HealthChanged;
        }

        private void OnDestroy()
        {
            health.HealthChanged -= HealthChanged;
        }

        private void HealthChanged()
        {
            if (!_isDeath && health.Current <= 0)
                Die();
        }

        private void Die()
        {
            _isDeath = true;
            
            if (_movement != null)
                _movement.enabled = false;

            if (_animator != null)
                _animator.PlayDeath();
            
            _stateMachine.Enter<GameOverState>();
        }
    }
}