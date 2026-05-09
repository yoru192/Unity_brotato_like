using System;
using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.Upgrade;
using CodeBase.Player.Movement;
using CodeBase.StaticData;
using CodeBase.StaticData.Hero;
using UnityEngine;

namespace CodeBase.Player
{
    public class PlayerStamina : MonoBehaviour, ISavedProgress
    {
        public event Action OnStaminaChanged;
        private IPersistentProgressService _persistentProgressService;
        private PlayerMovement _playerMovement;
        private PlayerControls _controls;
        private Character _character;
        private bool _isSprinting;

        private float _currentStamina;
        private float _maxStamina;
        [SerializeField] private float sprintMultiplier = 1.5f;
        private float _staminaRegenRate;
        private float _staminaUnregenRate = 10f;
        private IUpgradeService _upgradeService;
        private HeroStaticData _heroData;

        public float CurrentStamina => _currentStamina;
        public float MaxStamina => _maxStamina;

        public void Construct(IPersistentProgressService persistentProgressService, IUpgradeService upgradeService, HeroStaticData heroStaticData)
        {
            _persistentProgressService = persistentProgressService;
            _upgradeService = upgradeService;
            _heroData = heroStaticData;
            _upgradeService.OnUpgradeStamina += UpgradeStamina;
        }

        private void Awake()
        {
            _playerMovement = GetComponent<PlayerMovement>();
            _character = GetComponent<Character>();
            _controls = new PlayerControls();
            _controls.Player.Sprint.performed += ctx => _isSprinting = true;
            _controls.Player.Sprint.canceled += ctx => _isSprinting = false;
        }

        private void Start() { }

        private void OnEnable()
        {
            _controls.Enable();
        }

        private void OnDisable()
        {
            _controls.Disable();
            _upgradeService.OnUpgradeStamina -= UpgradeStamina;
        }
        private void Update()
        {
            bool canSprint = _currentStamina > 0f;
            bool isMoving = _playerMovement.GetComponent<Rigidbody2D>().linearVelocity.magnitude > 0.1f;
            float previousStamina = _currentStamina;

            if (_isSprinting && isMoving && canSprint)
            {
                _currentStamina -= _staminaUnregenRate * Time.deltaTime;
                _currentStamina = Mathf.Max(0f, _currentStamina);
                _playerMovement.SpeedMultiplier = sprintMultiplier;
                _playerMovement.IsSprinting = true;
                _character.SetState(CharacterState.Run);
            }
            else
            {
                _playerMovement.SpeedMultiplier = 1f;
                _playerMovement.IsSprinting = false;
                if (_currentStamina < _maxStamina)
                {
                    _currentStamina += _staminaRegenRate * Time.deltaTime;
                    _currentStamina = Mathf.Min(_maxStamina, _currentStamina);
                }
            }

            if (!Mathf.Approximately(previousStamina, _currentStamina))
                OnStaminaChanged?.Invoke();
        }

        private void UpgradeStamina()
        {
            _maxStamina = _persistentProgressService.Progress.playerState.maxStamina;
            _staminaRegenRate = _persistentProgressService.Progress.playerState.regenRateStamina;
            
        }

        public void LoadProgress(PlayerProgress progress)
        {
            if (progress.playerState.maxStamina == 0)
            {
                progress.playerState.maxStamina = _heroData.maxStamina;
                progress.playerState.currentStamina = _heroData.maxStamina;
            }

            _maxStamina = progress.playerState.maxStamina;
            _currentStamina = progress.playerState.currentStamina;
            _staminaRegenRate =  progress.playerState.regenRateStamina == 0
                ? _heroData.regenRateStamina
                : progress.playerState.regenRateStamina;
            OnStaminaChanged?.Invoke();
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            progress.playerState.currentStamina = _currentStamina;
            progress.playerState.maxStamina = _maxStamina;
            progress.playerState.regenRateStamina = _staminaRegenRate;
        }
    }
}