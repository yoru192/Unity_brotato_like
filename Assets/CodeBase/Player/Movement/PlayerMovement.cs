using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using CodeBase.Data;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Audio;
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
        private IAudioService _audioService;
        private AudioClipId? _currentMovementClip;

        public void Construct(HeroStaticData heroData)
        {
            _heroData = heroData;
            _audioService = AllServices.Container.Single<IAudioService>();
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
            StopMovementSound();
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
            UpdateMovementSound();
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

        private void UpdateMovementSound()
        {
            if (_audioService == null) return;

            bool isDead = _character != null && _character.GetState() >= CharacterState.DeathB;
            bool isMoving = !isDead && _rb.linearVelocity.magnitude > 0.1f;

            if (!isMoving)
            {
                StopMovementSound();
                return;
            }

            AudioClipId desiredClip = IsSprinting ? AudioClipId.Run : AudioClipId.Walk;
            if (_currentMovementClip == desiredClip) return;

            StopMovementSound();
            _audioService.PlayLoop(desiredClip);
            _currentMovementClip = desiredClip;
        }

        private void StopMovementSound()
        {
            if (_currentMovementClip == null) return;

            _audioService.StopLoop(_currentMovementClip.Value);
            _currentMovementClip = null;
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