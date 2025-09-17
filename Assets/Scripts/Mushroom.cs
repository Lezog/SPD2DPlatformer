using UnityEngine;

public class Mushroom : MonoBehaviour
{
    private PlayerMovement target;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float maxSpeed = 2f;
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private float heartDropChance = 0.3f;

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        target = FindFirstObjectByType<PlayerMovement>();
    }

    private void FixedUpdate()
    {
        if (target == null) Kill();

        float dirX = Mathf.Sign(target.transform.position.x - transform.position.x);

        rb.AddForce(new Vector2(dirX * moveSpeed, 0f), ForceMode2D.Force);

        Vector2 clamped = rb.linearVelocity;
        clamped.x = Mathf.Clamp(clamped.x, -maxSpeed, maxSpeed);
        rb.linearVelocity = new Vector2(clamped.x, rb.linearVelocity.y);

        if (dirX != 0)
            sprite.flipX = dirX < 0;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other != null && other.gameObject.CompareTag("Player"))
        {
            PlayerMovement player = other.gameObject.GetComponent<PlayerMovement>();
            if (player != null)
            {
                Vector2 direction = (player.transform.position - transform.position);
                player.TakeDamage(damage, direction, 1f);
                Kill();
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null && other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                Vector2 direction = (player.transform.position - transform.position);
                player.TakeDamage(0, direction, 1f);
                Kill();

            }
        }
    }

    public void Kill()
    {
        animator.SetTrigger("kill");
        if (Random.value <= heartDropChance && heartPrefab != null)
        {
            GameObject heart = Instantiate(heartPrefab, transform.position, Quaternion.identity);
            Rigidbody2D hrb = heart.GetComponent<Rigidbody2D>();
            if (hrb != null)
            {
                hrb.AddTorque(Random.Range(-20f, 20f));
                hrb.AddForce(Vector2.up * 100f);
                hrb.AddForce(Vector2.right * Random.Range(-10f, 10f));
            }
        }
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
