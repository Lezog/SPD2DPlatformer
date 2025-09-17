using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{

    [SerializeField] private int healingAmount = 2;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private GameObject collectionParticles;
    private Rigidbody2D rb;

    void Start()
    {
        StartCoroutine(destroyHeart());
        rb = GetComponent<Rigidbody2D>();
    }

    private IEnumerator destroyHeart()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            PlayerMovement player = collision.collider.GetComponent<PlayerMovement>();
            if (!player.isMaxHealth)
            {
                Instantiate(collectionParticles, transform.position, Quaternion.identity);
                rb.constraints = RigidbodyConstraints2D.FreezePosition;
                player.IncreaseHealth(healingAmount);
                Destroy(gameObject);
            }

        }
    }
}
