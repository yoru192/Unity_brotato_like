using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;

namespace CodeBase.Player.Movement
{
    public class PlayerMovement : MonoBehaviour, ISavedProgress
    {
        [SerializeField] private float moveSpeed = 5f;
        
        private Rigidbody2D _rb;
        private Character _character;
        private PlayerControls _controls;
        private Vector2 _moveInput;
        private IPersistentProgressService _persistentProgress;

        public void Construct(IPersistentProgressService persistentProgress)
        {
            _persistentProgress = persistentProgress;
        }
        
        
        public float MoveSpeed
        {
            get => _persistentProgress?.Progress.playerState?.moveSpeed ?? moveSpeed;
            set
            {
                if (_persistentProgress?.Progress.playerState != null && _persistentProgress?.Progress.playerState.moveSpeed != value)
                {
                    _persistentProgress.Progress.playerState.moveSpeed = value;
                }
            }
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _character = GetComponent<Character>();
            
            _controls = new PlayerControls();
            _controls.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
            _controls.Player.Move.canceled += ctx => _moveInput = Vector2.zero;
        }

        private void OnEnable()
        {
            _controls.Enable();
        }

        private void OnDisable()
        {
            _controls.Disable();
        }

        private void FixedUpdate()
        {
            Vector2 movement = _moveInput.normalized;
            _rb.linearVelocity = movement * MoveSpeed;
        }

        private void Update()
        {
            UpdateCharacterState();
            UpdateDirection();
        }

        private void UpdateCharacterState()
        {
            if (_character == null) return;

            if (_character.GetState() >= CharacterState.DeathB) return;

            if (_rb.linearVelocity.magnitude > 0.1f)
            {
                _character.SetState(CharacterState.Run);
            }
            else
            {
                _character.SetState(CharacterState.Idle);
            }
        }

        private void UpdateDirection()
        {
            if (_rb.linearVelocity.x != 0)
            {
                int facingDirection = _rb.linearVelocity.x > 0 ? 1 : -1;
                transform.localScale = new Vector3(facingDirection, 1, 1);
            }
        }

        public void LoadProgress(PlayerProgress progress)
        {
            if (_persistentProgress != null) _persistentProgress.Progress.playerState = progress.playerState;

            if (_persistentProgress?.Progress.playerState.moveSpeed == 0)
            {
                _persistentProgress.Progress.playerState.moveSpeed = moveSpeed;
            }
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            progress.playerState.moveSpeed = MoveSpeed;
        }
    }
}
