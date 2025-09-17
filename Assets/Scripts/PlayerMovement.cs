using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//GitHub test change

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float airAcceleration = 6f;
    [SerializeField] private float maxRunSpeed = 11f;
    private float moveSpeed;
    private bool canMove;

    [Header("Jumping")]
    [SerializeField] private float jumpStrength = 10f;
    [SerializeField] private float wallJumpHeightIncrease = 5f;
    [SerializeField] private float wallJumpPushForce = 10f;
    private int wallJumpLimit = 2;

    [Header("Slide")]
    [SerializeField] private float slideSpeed = 12f;
    [SerializeField] private float slideDuration = 0.3f;
    [SerializeField] private float slideCooldown = 1f;
    private float slideTimer = 0f;
    private float slideCooldownTimer = 0f;

    [Header("Wall")]
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private float raySideOffset = 0.5f;

    [Header("Ground")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private float rayGroundOffset = 0.5f;
    [SerializeField] private GameObject spawnPoint;

    [Header("Player Combat")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private int maxHealth = 25;
    [SerializeField] private int GroundAttackDamage = 5;
    [SerializeField] private int AirAttackDamage = 8;
    [SerializeField] private int SlideAttackDamage = 3;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float slideRangeMult = 1.5f;
    [SerializeField] private float invunrabilityTime = 0.1f;
    private float attackTimer = 0f;
    private float attackStateDuration = 0.4f;
    private int currentHealth;
    public float airAttackRange = 2f;
    public bool isMaxHealth = true;
    private bool canTakeDamage = true;

    [Header("Stats")]
    [SerializeField] private int gems;
    [SerializeField] private int kills;

    [Header("UI Elements")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider attackSlider;
    [SerializeField] private TMP_Text gemCounter;

    [Header("SoundFx")]
    [SerializeField] private float baseStepInterval = 0.3f;
    [SerializeField] private AudioClip jumpSound, hitSound,missSound, takeDamageSound, pickupSound, runSound, slideSound, healSound;
    private float stepTimer;

    [Header("PhysicsLayers")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask ground;

    private float horizontalValue;
    private bool isTouchingWall;
    private bool isGrounded;
    public bool isSliding;
    private bool isAttacking;
    private int wallSide; // -1 = left, 1 = right
    private float originalColliderY;
    private Vector2 originalColliderOffset;

    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private AudioSource audioSource;

    public enum AttackType
    {
        Ground,
        Air,
        Slide
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        currentHealth = maxHealth;
        gemCounter.text = "" + gems;

        originalColliderY = capsuleCollider.size.y;
        originalColliderOffset = capsuleCollider.offset;
        canMove = true;
        canTakeDamage = true;
    }

    private void Update()
    {
        if (!isAttacking && canMove) horizontalValue = Input.GetAxisRaw("Horizontal");
        if (horizontalValue < 0 && !isAttacking) FlipSprite(true);
        if (horizontalValue > 0 && !isAttacking) FlipSprite(false);

        //Jump
        if (Input.GetButtonDown("Jump") && isGrounded &&canMove)
        {

            Jump(Vector2.up);
            wallJumpLimit = 2;
        }

        //WallJump
        if (Input.GetButtonDown("Jump") && isTouchingWall && !isGrounded && wallJumpLimit > 0)
        {
            Vector2 wallJumpDirection = new Vector2(-wallSide * wallJumpPushForce, wallJumpHeightIncrease);
            Jump(wallJumpDirection, true);
            wallJumpLimit--;
        }

        //Slide
        if (slideCooldownTimer > 0) slideCooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.LeftShift) && canMove && isGrounded && !isSliding && slideCooldownTimer <= 0f && Mathf.Abs(horizontalValue) > 0.01f )
            {
            StartSlide();
        }

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0)
            {
                EndSlide();
            }
        }

        if (Input.GetButtonDown("Fire1") && attackTimer <= 0f)
        {
            if (isGrounded && !isSliding)
                StartCoroutine(PerformGroundAttackAfterDelay(0.2f));  
            else if (!isGrounded)
                StartCoroutine(PerformAirAttackAfterDelay(0.2f));
            else if (isSliding)
                StartCoroutine(PerformSlideAttackAfterDelay(0.2f));
               

            isAttacking = true;
            attackTimer = attackCooldown;
            attackStateDuration = 0.4f;
        }

        attackTimer = Mathf.Max(attackTimer - Time.deltaTime, 0f);

        if (isAttacking)
        {
            attackStateDuration -= Time.deltaTime;
            if (attackStateDuration <= 0)
                isAttacking = false;
        }

        moveSpeed = Mathf.Abs(rb.linearVelocity.x);
        Vector2 stop = Vector2.zero;
        if (!canMove) rb.linearVelocity = stop;

        //Animator
        animator.SetFloat("moveSpeed", moveSpeed);
        animator.SetBool("isSliding",isSliding);
        animator.SetBool("isTouchingWall",isTouchingWall);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("fallSpeed", rb.linearVelocityY );
        animator.SetBool("isAttacking", isAttacking);

        //UI
        attackSlider.value = attackTimer;
        healthSlider.value = currentHealth;

        //Sound
        if (moveSpeed > 5f && isGrounded && !isSliding)
        {
            float stepInterval = baseStepInterval * (6.66f / moveSpeed);

            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlaySoundWithRandomPitch(runSound, 0.8f, 1.2f, 0.3f); ;
                stepTimer = stepInterval;
            }
        }
    }

    private void FixedUpdate()
    {
        Vector2 position = transform.position;

        // Ground check
        Vector2 groundOffset = new Vector2(0, rayGroundOffset);
        RaycastHit2D groundHit = Physics2D.Raycast(position + groundOffset, Vector2.down, groundCheckDistance, ground);
        isGrounded = groundHit.collider != null;

        // Wall check
        RaycastHit2D leftWall = Physics2D.Raycast(position + new Vector2(-raySideOffset, 0), Vector2.left, wallCheckDistance, ground);
        RaycastHit2D rightWall = Physics2D.Raycast(position + new Vector2(raySideOffset, 0), Vector2.right, wallCheckDistance, ground);

        isTouchingWall = false;
        wallSide = 0;

        if (leftWall.collider != null)
        {
            isTouchingWall = true;
            wallSide = -1;
        }
        else if (rightWall.collider != null)
        {
            isTouchingWall = true;
            wallSide = 1;
        }

        // Acceleration
        float targetSpeed = horizontalValue * maxRunSpeed;
        float accel = isGrounded ? acceleration : airAcceleration;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float movement = speedDiff * accel * Time.fixedDeltaTime;

        if (!isSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x + movement, rb.linearVelocity.y);

            // Friction
            if (horizontalValue == 0 && isGrounded && !isTouchingWall)
            {
                float friction = 20f;
                float newX = Mathf.MoveTowards(rb.linearVelocity.x, 0, friction * Time.fixedDeltaTime);
                rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
            }

            // Wallslide
            if (isTouchingWall && !isGrounded && rb.linearVelocity.y < 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
            }
        }
        else
        {
            // Slide
            float dir = Mathf.Sign(horizontalValue);
            rb.linearVelocity = new Vector2(dir * slideSpeed, rb.linearVelocity.y);
        }
    }

    private void FlipSprite(bool direction)
    {
        spriteRenderer.flipX = direction;
    }

    private void Jump(Vector2 direction, bool isWallJump = false)
    {
        PlaySoundWithRandomPitch(jumpSound, 0.9f, 1.1f, 0.3f); ;

        if (isWallJump)
        {
            rb.linearVelocity = direction;
        }
        else
        {
            float currentX = rb.linearVelocity.x;
            rb.linearVelocity = new Vector2(currentX, 0);
            rb.AddForce(Vector2.up * jumpStrength, ForceMode2D.Impulse);
        }
    }
    private void StartSlide()
    {
        PlaySoundWithRandomPitch(slideSound, 0.95f, 1.05f, 0.5f);
        isSliding = true;
        StartCoroutine(InvunrabilityFrame());
        slideTimer = slideDuration;
        slideCooldownTimer = slideCooldown;

        capsuleCollider.size = new Vector2(capsuleCollider.size.x, originalColliderY * 0.5f);
        capsuleCollider.offset = new Vector2(capsuleCollider.offset.x, originalColliderOffset.y - (originalColliderY - capsuleCollider.size.y) / 2);

        float dir = Mathf.Sign(horizontalValue);
        if (moveSpeed < slideSpeed)
            rb.linearVelocity = new Vector2(dir * slideSpeed, rb.linearVelocity.y);
 
    }

    private void EndSlide()
    {
        isSliding = false;

        capsuleCollider.size = new Vector2(capsuleCollider.size.x, originalColliderY);
        capsuleCollider.offset = originalColliderOffset;
    }

    private IEnumerator InvunrabilityFrame()
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(invunrabilityTime);
        canTakeDamage = true;
    }

    public void TakeDamage(int damage, Vector2 hitDirection,float knockbackMult)
    {
        if (canTakeDamage)
        {
            PlaySoundWithRandomPitch(takeDamageSound, 0.95f, 1.05f, 0.5f);

            currentHealth = currentHealth - damage;
            isMaxHealth = false;
            animator.SetTrigger("dmg");

            rb.linearVelocity = Vector2.zero;
            Vector2 knowbackDir = (hitDirection + Vector2.up * 1.5f * knockbackMult).normalized;
            rb.AddForce(knowbackDir * knockbackForce * knockbackMult, ForceMode2D.Impulse);

            if (currentHealth < 0)
            {
                StartCoroutine(Respawn());
            }
        }
    }

    public IEnumerator Respawn()
    {
        animator.SetTrigger("death");
        canMove = false;

        yield return new WaitForSeconds(1);

        gameObject.transform.position = spawnPoint.transform.position;
        currentHealth = maxHealth;
        canMove = true;
        animator.SetTrigger("reset");
    }

    public void IncreaseHealth(int amount)
    {
        PlaySoundWithRandomPitch(healSound, 0.95f, 1.05f, 0.5f);
        if (currentHealth + amount > maxHealth)
        {
            currentHealth = maxHealth;
            isMaxHealth = true;
        }
        else
        {
            currentHealth += amount;
            isMaxHealth = false;
        }
    }

    public void GiveGem(int amount)
    {
        PlaySoundWithRandomPitch(pickupSound, 0.95f, 1.05f, 0.3f);
        gems += amount;
        gemCounter.text = "" + gems;
    }

    private IEnumerator PerformAirAttackAfterDelay(float delay)
    {
        animator.SetTrigger("AirAttack");
        yield return new WaitForSeconds(delay);

        Vector2 origin = transform.position;
        Vector2 AirfacingDir = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        float[] angles = spriteRenderer.flipX ? new float[] { 0f, 45f, 90f } : new float[] { 0f, -45f, -90f };

        List<Enemy> hitEnemies = new List<Enemy>();

        foreach (float angle in angles)
        {
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(
                AirfacingDir.x * Mathf.Cos(rad) - AirfacingDir.y * Mathf.Sin(rad),
                AirfacingDir.x * Mathf.Sin(rad) + AirfacingDir.y * Mathf.Cos(rad)
            ).normalized;

            RaycastHit2D hit = Physics2D.Raycast(origin, dir, airAttackRange, LayerMask.GetMask("Enemy"));
            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                if (enemy != null && !hitEnemies.Contains(enemy))
                    hitEnemies.Add(enemy);
            }
            else if (hit.collider != null && hit.collider.CompareTag("Mushroom"))
            {
                Mushroom mushroom = hit.collider.GetComponent<Mushroom>();
                mushroom.Kill();
                PlaySoundWithRandomPitch(hitSound, 0.95f, 1.05f, 0.5f);
            }

                Debug.DrawRay(origin, dir * airAttackRange, Color.red, 0.2f);
        }

        if (hitEnemies.Count > 0)
        {
            foreach (Enemy enemy in hitEnemies)
            {
                enemy.TakeDamage(AirAttackDamage);
                PlaySoundWithRandomPitch(hitSound, 0.95f, 1.05f, 0.5f); ;
            }
        }
        else { PlaySoundWithRandomPitch(missSound, 0.95f, 1.05f, 0.5f); ; }
    }

    private IEnumerator PerformSlideAttackAfterDelay(float delay)
    {
        animator.SetTrigger("SlideAttack");
        yield return new WaitForSeconds(delay);

        Vector2 origin = transform.position;
        Vector2 facingDir = spriteRenderer.flipX ? Vector2.left : Vector2.right;

        RaycastHit2D other = Physics2D.Raycast(origin, facingDir, attackRange * slideRangeMult, LayerMask.GetMask("Enemy"));
        Debug.DrawRay(origin, facingDir * (attackRange * slideRangeMult), Color.blue, 0.2f);

        if (other.collider != null && other.collider.CompareTag("Enemy"))
        {
            other.collider.GetComponent<Enemy>().TakeDamage(SlideAttackDamage);
            PlaySoundWithRandomPitch(hitSound, 0.95f, 1.05f, 0.5f);
        }
        else if (other.collider != null && other.collider.CompareTag("Mushroom"))
        {
            other.collider.GetComponent<Mushroom>().Kill();
            PlaySoundWithRandomPitch(hitSound, 0.95f, 1.05f, 0.5f);
            StartCoroutine(SlowSpeed());
        }
        else
        {
            PlaySoundWithRandomPitch(missSound, 0.95f, 1.05f, 0.5f);
        }
    }

    private IEnumerator PerformGroundAttackAfterDelay(float delay)
    {
        animator.SetTrigger("GroundAttack");
        yield return new WaitForSeconds(delay);

        Vector2 origin = transform.position;
        Vector2 facingDir = spriteRenderer.flipX ? Vector2.left : Vector2.right;

        float groundAttackRange = attackRange;
        if (moveSpeed > 6) groundAttackRange = attackRange * 1.5f;

        RaycastHit2D other = Physics2D.Raycast(origin, facingDir, groundAttackRange, LayerMask.GetMask("Enemy"));
        if (moveSpeed < 6) Debug.DrawRay(origin, facingDir * groundAttackRange, Color.green, 0.2f);

        if (other.collider != null && other.collider.CompareTag("Enemy"))
        {
            other.collider.GetComponent<Enemy>().TakeDamage(GroundAttackDamage);
            PlaySoundWithRandomPitch(hitSound, 0.95f, 1.05f, 0.5f);
            StartCoroutine(SlowSpeed());
        }
        else if (other.collider != null && other.collider.CompareTag("Mushroom"))
        {
            other.collider.GetComponent<Mushroom>().Kill();
            PlaySoundWithRandomPitch(hitSound, 0.95f, 1.05f, 0.5f);
            StartCoroutine(SlowSpeed());
        }
        else { PlaySoundWithRandomPitch(missSound, 0.95f, 1.05f, 0.5f); }
    }
    private IEnumerator SlowSpeed()
    {
        float originalSpeed = maxRunSpeed;
        maxRunSpeed *= 0.2f;
        yield return new WaitForSeconds(0.4f);
        maxRunSpeed = originalSpeed;
    }

    public void countKill()
    {
        kills++;
    }

    private void PlaySoundWithRandomPitch(AudioClip clip, float minPitch, float maxPitch, float volume = 1f)
    {
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(clip, volume);
        audioSource.pitch = 1f;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 pos = transform.position;

        // ground check
        Vector2 groundOffset = new Vector2(0, rayGroundOffset);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pos + groundOffset, pos + groundOffset + Vector2.down * groundCheckDistance);

        // wall check
        Vector2 rightOffset = new Vector2(raySideOffset, 0);
        Vector2 leftOffset = new Vector2(-raySideOffset, 0);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pos + rightOffset, pos + rightOffset + Vector2.right * wallCheckDistance);
        Gizmos.DrawLine(pos + leftOffset, pos + leftOffset + Vector2.left * wallCheckDistance);

    }
}
