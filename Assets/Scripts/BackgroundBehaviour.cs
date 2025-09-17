using UnityEngine;

public class BackgroundBehaviour : MonoBehaviour
{
    [SerializeField] private float parallaxAmount = 0.5f;
    private Transform cam;
    private Vector3 lastCamPos;

    void Start()
    {
        cam = Camera.main.transform;
        lastCamPos = cam.position;
    }

    void LateUpdate()
    {
        Vector3 delta = cam.position - lastCamPos;
        transform.position += new Vector3(delta.x * parallaxAmount, delta.y * parallaxAmount, 0);
        lastCamPos = cam.position;
    }
}
