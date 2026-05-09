using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.StaticData.Hero;
using UnityEngine;

namespace CodeBase.Player.Movement
{
    public class PlayerMovement : MonoBehaviour, ISavedProgress
    {
        private Rigidbody2D _rb;
        private Character _character;
        private PlayerControls _controls;
        private Vector2 _moveInput;
        private HeroStaticData _heroData;
        private State _state;

        public void Construct(HeroStaticData heroData)
        {
            _heroData = heroData;
        }
        
        public bool IsSprinting { get; set; }
        public float SpeedMultiplier { get; set; } = 1f;

        private float BaseMoveSpeed => _state?.moveSpeed > 0 ? _state.moveSpeed : _heroData?.moveSpeed > 0 ? _heroData.moveSpeed : 1f;


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
            _rb.linearVelocity = movement * (BaseMoveSpeed * SpeedMultiplier);
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
            if (IsSprinting) return;

            if (_rb.linearVelocity.magnitude > 0.1f)
                _character.SetState(CharacterState.Walk);
            else
                _character.SetState(CharacterState.Idle);
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
            _state = progress.playerState;
            if (_state.moveSpeed == 0 && _heroData.moveSpeed > 0)
                _state.moveSpeed = _heroData.moveSpeed;
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            progress.playerState.moveSpeed = BaseMoveSpeed;
        }
    }
}