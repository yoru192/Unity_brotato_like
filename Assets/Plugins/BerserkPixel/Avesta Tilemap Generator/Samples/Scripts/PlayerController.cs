using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 20.0f;
    [Tooltip("Used for diagonal movement")]
    [SerializeField] private float moveLimiter = 0.7f;

    private Vector3 _movement;
    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Update ()
    {
        // rotation
        Vector3 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

        Vector2 directionToLookAt = new Vector2(
            mousePosition.x - transform.position.x,
            mousePosition.y - transform.position.y
        ).normalized;

        transform.up = directionToLookAt;
        
        // movement
        _movement = Vector3.right * Input.GetAxisRaw("Horizontal") + Vector3.up * Input.GetAxisRaw("Vertical");
        _movement.z = 0;
        
        if (_movement.magnitude != 0) // Check for diagonal movement
        {
            // limit movement speed diagonally, so you move at 70% speed
            _movement *= moveLimiter;
        }

        _movement *= speed * Time.deltaTime;
        
        transform.Translate(_movement);
    }
}
