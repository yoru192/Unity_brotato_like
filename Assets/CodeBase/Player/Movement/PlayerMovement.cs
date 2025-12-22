using UnityEngine;

namespace CodeBase.Player.Movement
{
    public class PlayerMovement : MonoBehaviour
    {
        private Rigidbody2D _rb;
        public float moveSpeed = 5f;

        private PlayerControls _controls;
        private Vector2 _moveInput;

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
            _rb.linearVelocity = movement * moveSpeed;
        }
    }
}