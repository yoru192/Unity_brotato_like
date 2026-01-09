using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;

namespace CodeBase.Player.Movement
{
    public class PlayerMovement : MonoBehaviour, ISavedProgress
    {
        private Rigidbody2D _rb;
        public float moveSpeed = 5f;
        public float MoveSpeed
        {
            get => _state?.moveSpeed ?? moveSpeed;
            set
            {
                if(_state != null && _state.moveSpeed != value)
                {
                    _state.moveSpeed = value;
                }
            }
        }
        private PlayerControls _controls;
        private Vector2 _moveInput;
        private State _state;

        private void Awake()
        {
            _controls = new PlayerControls();

            _controls.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
            _controls.Player.Move.canceled  += ctx => _moveInput = Vector2.zero;
        }

        private void OnEnable()
        {
            _controls.Enable();
        }

        private void OnDisable()
        {
            _controls.Disable();
        }

        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            Vector2 movement = _moveInput.normalized;
            _rb.linearVelocity = movement * MoveSpeed;
        }

        public void LoadProgress(PlayerProgress progress)
        {
            _state = progress.playerState;
            if (_state.moveSpeed == 0)
            {
                _state.moveSpeed = moveSpeed;
            }
        }
    
        public void UpdateProgress(PlayerProgress progress)
        {
            progress.playerState.moveSpeed = MoveSpeed;
        }
    }
}