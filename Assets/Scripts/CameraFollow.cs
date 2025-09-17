using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0,-10f,5f);
    [SerializeField] private float smoothing = 1.0f;

    void LateUpdate()
    {
        Vector3 newPosition = Vector3.Lerp(transform.position, target.position + offset,smoothing * Time.deltaTime);
        transform.position = newPosition;
    }
}
