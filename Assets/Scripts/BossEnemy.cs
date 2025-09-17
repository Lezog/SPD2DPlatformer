using System.Collections;
using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 300;
    [SerializeField] private int currentHealth;
    [SerializeField] private float attackWindup = 0.25f;
    [SerializeField] private float attackRecovery = 0.8f;
    [SerializeField] private float attackBreak = 3f;

    [Header("Attacking")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float spawnTime = 5f;
    [SerializeField] private int attackCounter;

    [Header("SwingAttack")]
    [SerializeField] private GameObject swingAreaColliderHigh;
    [SerializeField] private GameObject swingAreaColliderMid;
    [SerializeField] private GameObject swingAreaColliderLow;
    [SerializeField] private GameObject swingAreaColliderEnd;
    [SerializeField] private GameObject swingArea;
    [SerializeField] private int swingDamage;
    [SerializeField] private float swingAreaOffset = 1f;
    [SerializeField] private int swingChancePercentage = 50;
    [SerializeField] private bool playerInSwingRange;
    [SerializeField] private float knockbackMult = 2f;
    private bool hasSwung;

    [Header("SummonAttack")]
    [SerializeField] private int summonAmount = 5;
    [SerializeField] private float summonRange = 8f;
    [SerializeField] private float groundYLevel;
    [SerializeField] private GameObject summonCircle;

    [Header("Teleporting")]
    [SerializeField] private Transform[] teleportPoints;

    [Header("Spawnables")]
    [SerializeField] private GameObject minion;
    [SerializeField] private GameObject portalVertical;
    [SerializeField] private GameObject portalHorizontal;
    [SerializeField] private GameObject fireBall; //spawn ground fire in fireball script

    [Header("Drops & FX")]
    [SerializeField] private GameObject key;
    [SerializeField] private GameObject deathEffect;

    [Header("Player")]
    [SerializeField] private GameObject player;
    private PlayerMovement pm;

    [Header("More booleans")]
    [SerializeField] private bool isAttacking;
    [SerializeField] private bool isDead;
    [SerializeField] private bool canAttack;

    private Coroutine attackRoutine;

    private Animator animator;
    private SpriteRenderer sprite;
    private Rigidbody2D rb;

    private void Start()
    {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        pm = player.GetComponent<PlayerMovement>();

        swingArea.SetActive(false);
        swingAreaColliderHigh.SetActive(false);
        swingAreaColliderMid.SetActive(false);
        swingAreaColliderLow.SetActive(false);
        swingAreaColliderEnd.SetActive(false);

        StartCoroutine(spawnSequence());
        
    }

    private void FixedUpdate()
    {
        if (player != null && !isDead)
        {
            //always face the player + flip attack area 
            UpdateFacing();
        }

        if (!isAttacking && canAttack && !isDead)
        {
            attackRoutine = StartCoroutine(AttackPattern());
        }
    }

    private IEnumerator spawnSequence() //first spawn
    {
        yield return new WaitForSeconds(spawnTime);
        swingArea.SetActive(true);
        currentHealth = maxHealth;
        isAttacking = false;
        isDead = false;
        canAttack = true;
        attackCounter = 0;
    }
    private void UpdateFacing()
    {
        bool flip = player.transform.position.x < transform.position.x;
        sprite.flipX = flip;

        GameObject[] colliders = new GameObject[]
        {
            swingArea,
            swingAreaColliderHigh,
            swingAreaColliderMid,
            swingAreaColliderLow,
            swingAreaColliderEnd,
            
        };

        foreach (GameObject col in colliders) // go through all colliders and offset with sprite
        {
            if (col == null) continue;

            Vector3 colScale = col.transform.localScale;
            colScale.x = flip ? -1 : 1;
            col.transform.localScale = colScale;

            Vector3 colPos = col.transform.localPosition;
            colPos.x = flip ? -swingAreaOffset : swingAreaOffset;
            col.transform.localPosition = colPos;
        }

    }
    private IEnumerator AttackPattern() //logic for attacks in row
    {
        canAttack = false;
        attackCounter = 0;

        int randomRange = Random.Range(3, 5);   // attack combo is 3 to 5 attacks long before break
        while (attackCounter <randomRange)
        {
            yield return StartCoroutine(DoAttack());
            attackCounter++;
        }

        Teleport();  //teleport away after attack sequence
        yield return new WaitForSeconds(attackBreak);

        canAttack = true;
        attackRoutine = null;
    }
    private BossAttackType ChooseAttack() //random chance to do a different attack
    {

        if (playerInSwingRange) //if in range chance to do swing attack
        {
            int roll = Random.Range(0, 100);
            if (roll < swingChancePercentage) return BossAttackType.Melee;
        }

        int choice = Random.Range(0, 3);
        return choice switch
        {
            0 => BossAttackType.SpawnMinions,
            1 => BossAttackType.SkyFire,
            _ => BossAttackType.FireBall
        };
    }
    private enum BossAttackType
    {
        Melee,
        SpawnMinions,
        SkyFire,
        FireBall
    }
    private IEnumerator DoAttack()
    {
        isAttacking = true;

        BossAttackType chosenAttack = ChooseAttack();

        switch (chosenAttack)
        {
            case BossAttackType.Melee: //note: have all the animations call attacks
                animator.SetTrigger("swing");
                break;
            case BossAttackType.SpawnMinions:
                animator.SetTrigger("summon");
                break;
            case BossAttackType.SkyFire:
                animator.SetTrigger("skyfire");
                skyAttack();
                break;
            case BossAttackType.FireBall:
                animator.SetTrigger("fireball");
                fireballAttack();
                break;
        }

        yield return new WaitForSeconds(attackWindup);
        
        isAttacking = false;
        hasSwung = false;
        
        yield return new WaitForSeconds(attackRecovery);
 
    }

    public void OnAttackAreaEnter(Collider2D other) { playerInSwingRange = true; } 
    public void OnAttackAreaExit(Collider2D other) { playerInSwingRange = false; }
    public void swingHigh() { swingAreaColliderHigh.SetActive(true); }  //activate and disable hit collider frame by frame in anim
    public void swingMid() { swingAreaColliderMid.SetActive(true); swingAreaColliderHigh.SetActive(false); }
    public void swingLow() { swingAreaColliderLow.SetActive(true); swingAreaColliderMid.SetActive(false); }
    public void swingEnd() { swingAreaColliderEnd.SetActive(true); swingAreaColliderLow.SetActive(false); Invoke(nameof(DisableSwingColliders), 0.3f); }
    private void DisableSwingColliders()
    {
        swingAreaColliderHigh.SetActive(false);
        swingAreaColliderMid.SetActive(false);
        swingAreaColliderLow.SetActive(false);
        swingAreaColliderEnd.SetActive(false);
    }

    public void swingAttack() 
    {
        if (!hasSwung)
        {
                Vector2 hitDirection = player.transform.position - transform.position;
                pm.TakeDamage(swingDamage, hitDirection, knockbackMult);

            hasSwung = true;
        }
        DisableSwingColliders();
    }

    public void summonAttack()
    {
        for (int i = 0; i < summonAmount; i++)
        {
            float randomX = Random.Range(-summonRange, summonRange);
            Vector3 spawnPos = new Vector3(transform.position.x + randomX, groundYLevel, 0f);
            Instantiate(minion, spawnPos, Quaternion.identity);
        }
    }
    public void skyAttack()
    {

    }
    public void fireballAttack()
    {

    }
    private void Teleport()
    {

    }

    public void TakeDamage(int amount)
    {
        // maybe add way to stun while attacking ex. when boss is not attacking
        currentHealth -= amount;
        if (currentHealth <= 0 && !isDead)
        {
            StartCoroutine(KillObject());
        }
    }

    private IEnumerator KillObject()
    {
        isDead = true;
        animator.SetTrigger("kill");

        yield return new WaitForSeconds(0.5f);

        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        if (key != null)
        {
            GameObject keyAsset = Instantiate(key, transform.position, Quaternion.identity);
            Rigidbody2D keyrb = keyAsset.GetComponent<Rigidbody2D>();
            keyrb.AddTorque(Random.Range(-20f, 20f));
            keyrb.AddForce(Vector2.up * 100f);
            keyrb.AddForce(Vector2.right * Random.Range(-10f, 10f));
        }

        Destroy(gameObject);
    }
}