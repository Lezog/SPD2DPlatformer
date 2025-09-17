using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int damage = 3;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int currentHealth;

    [Header("Attack")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackWindup = 0.25f;
    [SerializeField] private float attackRecovery = 0.8f;
    [SerializeField] private float knockbackMult = 1f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Drops & FX")]
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private float heartDropChance = 0.3f;
    [SerializeField] private QuestBoard quests;
    [SerializeField] private GameObject deathEffect;

    private bool isAttacking;
    private bool isDead;
    public bool canAttack;

    private Coroutine attackRoutine;

    private Animator animator;
    private SpriteRenderer sprite;
    private Rigidbody2D rb;

    private void Start()
    {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        quests = GameObject.FindWithTag("QuestBoard").GetComponent<QuestBoard>();

        currentHealth = maxHealth;
        isAttacking = false;
        isDead = false;
        canAttack = true;
    }

    private void Update()
    {
        animator.SetBool("isAttacking", isAttacking);
        animator.SetFloat("isMoving", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("isDead",isDead);
    }

    private void FixedUpdate()
    {
        if (!isAttacking && canAttack)
        {
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
            sprite.flipX = moveSpeed < 0;

            Vector2 dir = sprite.flipX ? Vector2.left : Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, attackRange, playerLayer);
            Debug.DrawRay(transform.position, dir * attackRange, Color.red);

            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                PlayerMovement pm = hit.collider.GetComponent<PlayerMovement>();
                if (pm != null && attackRoutine == null)
                {
                    attackRoutine = StartCoroutine(DoAttack(pm));
                }
            }
        }
    }

    private IEnumerator DoAttack(PlayerMovement player)
    {
        isAttacking = true;
        animator.SetTrigger("attack");

        yield return new WaitForSeconds(attackWindup);
        isAttacking = false;

        yield return new WaitForSeconds(attackRecovery);

        canAttack = true;
        attackRoutine = null;
    }

    public void DealDamage()
    {
        if (!isAttacking) return;

        Vector2 baseDir = sprite.flipX ? Vector2.left : Vector2.right;
        Vector2[] directions =
        {
        baseDir,
        (baseDir + Vector2.up * 0.5f).normalized,
        (baseDir + Vector2.up * 1.2f).normalized
    };

        foreach (var d in directions)
        {
            var h = Physics2D.Raycast(transform.position, d, attackRange * 1.2f, playerLayer);
            if (h.collider != null && h.collider.CompareTag("Player"))
            {
                Vector2 facingDir = (h.collider.transform.position - transform.position).normalized;
                h.collider.GetComponent<PlayerMovement>().TakeDamage(damage, facingDir,knockbackMult);
                break;
            }
        }
    }

    private void DoContactAttack(GameObject player)
    {
        if (isDead) return;
        Vector2 dir = (player.transform.position - transform.position).normalized;
        player.GetComponent<PlayerMovement>()?.TakeDamage(damage - 2, dir,knockbackMult);
        TakeDamage(4);
    }

    private IEnumerator KillObject()
    {
        isDead = true;
        Vector3 offset = transform.position + new Vector3(0, 0.4f, 0.1f);
        if (deathEffect != null) Instantiate(deathEffect, offset, Quaternion.identity);

        animator.SetTrigger("kill");
        moveSpeed = 0;
        isAttacking = false;

        quests?.addKill();
        yield return new WaitForSeconds(0.5f);

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

        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("EnemyBlock"))
        {
            moveSpeed = -moveSpeed;
        }

        if (collision.gameObject.CompareTag("Player") && !isDead)
        {
            if (!isAttacking)
            {
                DoContactAttack(collision.gameObject);
            }
        }
    }
    private IEnumerator AttackLock(float duration)
    {
        canAttack = false;
        yield return new WaitForSeconds(duration);
        canAttack = true;
    }
    public void TakeDamage(int amount)
    {
        if (attackRoutine != null) //cancel attack and reset cooldown
        {
            StopCoroutine(attackRoutine); 
            attackRoutine = null;
        }

        StartCoroutine(AttackLock(0.3f));

        isAttacking = false;
        animator.ResetTrigger("attack");
        animator.SetTrigger("dmg");

        //flip on backstab
        Vector2 dir = sprite.flipX ? Vector2.left : Vector2.right;
        Vector2 oppositeDir = -dir;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, oppositeDir, attackRange * 2, playerLayer);
        Debug.DrawRay(transform.position, oppositeDir * attackRange * 2, Color.magenta);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            moveSpeed *= -1;
            sprite.flipX = moveSpeed < 0;
        }

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            StartCoroutine(KillObject());
        }
    }
}
