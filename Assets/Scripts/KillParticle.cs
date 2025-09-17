using UnityEngine;

public class KillParticle : MonoBehaviour
{
    private float lifetime = 1f;
    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
