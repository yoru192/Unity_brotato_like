using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Player.Movement
{
    public class PlayerMovement : MonoBehaviour, ISavedProgress
    {
        private Rigidbody2D _rb;
        private Character _character;
        private PlayerControls _controls;
        private Vector2 _moveInput;
        private IPersistentProgressService _persistentProgress;
        private PlayerStaticData _playerData;

        public void Construct(IPersistentProgressService persistentProgress, PlayerStaticData playerData)
        {
            _persistentProgress = persistentProgress;
            _playerData = playerData;
        }
        
        
        public float MoveSpeed
        {
            get
            {
                if (_persistentProgress?.Progress?.playerState == null)
                    return _playerData != null ? _playerData.moveSpeed : 0f;

                return _persistentProgress.Progress.playerState.moveSpeed == 0
                    ? _playerData.moveSpeed
                    : _persistentProgress.Progress.playerState.moveSpeed;
            }
            set
            {
                if (_persistentProgress?.Progress?.playerState == null)
                    return;

                if (_persistentProgress.Progress.playerState.moveSpeed != value)
                    _persistentProgress.Progress.playerState.moveSpeed = value;
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
            if(MoveSpeed > 0)
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
            if (progress.playerState.moveSpeed > 0)
                MoveSpeed = progress.playerState.moveSpeed;
        }


        public void UpdateProgress(PlayerProgress progress)
        {
            progress.playerState.moveSpeed = MoveSpeed;
        }
    }
}
