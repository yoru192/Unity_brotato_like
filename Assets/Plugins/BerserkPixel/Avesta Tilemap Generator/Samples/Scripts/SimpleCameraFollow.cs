using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = Vector3.back;

    private void LateUpdate()
    {
        transform.position = target.position + offset;
    }
}
