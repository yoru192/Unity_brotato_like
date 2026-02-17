using System;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.Upgrade;
using CodeBase.Player.Movement;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Player
{
    public class PlayerStamina : MonoBehaviour, ISavedProgress
    {
        public event Action OnStaminaChanged;
        private IPersistentProgressService _persistentProgressService;
        private PlayerMovement _playerMovement;
        private PlayerControls _controls;
        private float _normalSpeed = 5f;
        private bool _isSprinting;

        private float _currentStamina;
        private float _maxStamina;
        [SerializeField] private float sprintMultiplier = 1.5f;
        private float _staminaRegenRate;
        private float _staminaUnregenRate = 10f;
        private IUpgradeService _upgradeService;
        private PlayerStaticData _playerData;

        public float CurrentStamina => _currentStamina;
        public float MaxStamina => _maxStamina;

        public void Construct(IPersistentProgressService persistentProgressService, IUpgradeService upgradeService, PlayerStaticData playerStaticData)
        {
            _persistentProgressService = persistentProgressService;
            _upgradeService = upgradeService;
            _playerData = playerStaticData;
            _upgradeService.OnUpgradeStamina += UpgradeStamina;
        }

        private void Awake()
        {
            _playerMovement = GetComponent<PlayerMovement>();
            _controls = new PlayerControls();
            _controls.Player.Sprint.performed += ctx => _isSprinting = true;
            _controls.Player.Sprint.canceled += ctx => _isSprinting = false;
        }

        private void Start()
        {
            // _normalSpeed = _playerMovement.MoveSpeed;
        }

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
                _playerMovement.MoveSpeed = _normalSpeed * sprintMultiplier;
            }
            else
            {
                if (_currentStamina < _maxStamina)
                {
                    _currentStamina += _staminaRegenRate * Time.deltaTime;
                    _currentStamina = Mathf.Min(_maxStamina, _currentStamina);
                }
                _playerMovement.MoveSpeed = _normalSpeed;
            }
            
            if (!Mathf.Approximately(previousStamina, _currentStamina))
            {
                OnStaminaChanged?.Invoke();
            }
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
                progress.playerState.maxStamina = _playerData.maxStamina;
                progress.playerState.currentStamina = _playerData.maxStamina;
            }

            _maxStamina = progress.playerState.maxStamina;
            _currentStamina = progress.playerState.currentStamina;
            _staminaRegenRate =  progress.playerState.regenRateStamina == 0
                ? _playerData.regenRateStamina
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
