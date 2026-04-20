using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Core")]
    public HealthBar healthBar;
    public int playerIndex = 1;
    public int lives = 3;
    public float damagePercent = 0f;
    public float maxDamagePercent = 999f;
    public GameObject WinUI;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Ground / Platform")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public Transform platformCheck;
    public float platformCheckRadius = 0.2f;
    public LayerMask platformLayer;

    private bool isGrounded;
    private bool isPlatform;

    [Header("Wall")]
    public float wallCheckDistance = 0.47f;
    public float wallSlideSpeed = 2f;
    public float wallJumpCooldownTime = 0.3f;

    private bool isTouchingWall;
    private bool isWallSliding;
    private float wallJumpCooldown;

    [Header("Jump")]
    public int extraJumpsValue = 1;
    private int extraJumps;

    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    [Header("Platform Drop")]
    private bool canPassThroughPlatform = false;
    private Collider2D currentPlatformCollider;
    private float passThroughTimer = 0f;
    public float passThroughDuration = 0.3f;

    [Header("Combat")]
    public float attackDuration = 1f;
    private float attackTimer;
    private bool isAttacking;

    public float hitstunDuration = 0.3f;
    private float hitstunTimer;
    private bool isInHitstun;

    [Header("References")]
    public Transform respawnPoint;
    public Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private AudioSource audioSource;

    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite attackSprite;
    public Sprite attackSprite2;

    [Header("Audio")]
    public AudioClip jumpClip;
    public AudioClip winClip;

    private bool hasLost = false;
    void Start()
    {
        Application.targetFrameRate = 60;

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        extraJumps = extraJumpsValue;

        if (healthBar == null)
        {
            healthBar = FindFirstObjectByType<HealthBar>();
        }

        // Initialize health bar correctly
        if (healthBar != null)
        {
            healthBar.SetHealth(damagePercent);  // Show current damage %
        }

        if (normalSprite != null)
            spriteRenderer.sprite = normalSprite;
    }

    void Update()
    {
        float moveInput = GetHorizontal();

        HandleTimers();
        HandlePlatformDrop();

        if (!isAttacking && !isInHitstun)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }

        FlipSprite(moveInput);
        HandleJump();
        HandleAttack();
        HandleWallSlide();

        SetGravity();
        SetAnimation(moveInput);

        if (transform.position.y < -12)
            Die();
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!canPassThroughPlatform)
            isPlatform = Physics2D.OverlapCircle(platformCheck.position, platformCheckRadius, platformLayer);
        else
            isPlatform = false;

        isTouchingWall = Physics2D.Raycast(
            transform.position,
            spriteRenderer.flipX ? Vector2.left : Vector2.right,
            wallCheckDistance,
            groundLayer
        );
    }

    // ---------------- INPUT ----------------

    float GetHorizontal()
    {
        if (playerIndex == 1)
        {
            if (Input.GetKey(KeyCode.A)) return -1;
            if (Input.GetKey(KeyCode.D)) return 1;
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftArrow)) return -1;
            if (Input.GetKey(KeyCode.RightArrow)) return 1;
        }
        return 0;
    }

    bool JumpPressed()
    {
        return playerIndex == 1
            ? Input.GetKeyDown(KeyCode.W)
            : Input.GetKeyDown(KeyCode.UpArrow);
    }

    bool AttackPressed()
    {
        return playerIndex == 1
            ? Input.GetKeyDown(KeyCode.J)
            : Input.GetKeyDown(KeyCode.RightControl);
    }

    bool DownPressed()
    {
        return playerIndex == 1
            ? Input.GetKey(KeyCode.S)
            : Input.GetKey(KeyCode.DownArrow);
    }

    // ---------------- MOVEMENT ----------------

    void HandleJump()
    {
        if (isGrounded || isPlatform)
            extraJumps = extraJumpsValue;

        if (JumpPressed() && !isAttacking && !isInHitstun)
        {
            if (isGrounded || isPlatform)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                PlaySFX(jumpClip);
            }
            else if (isWallSliding && wallJumpCooldown <= 0f)
            {
                float dir = spriteRenderer.flipX ? -1 : 1;
                rb.linearVelocity = new Vector2(dir * 8f, jumpForce);
                spriteRenderer.flipX = !spriteRenderer.flipX;
                wallJumpCooldown = wallJumpCooldownTime;
                isWallSliding = false;
                PlaySFX(jumpClip);
            }
            else if (extraJumps > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                extraJumps--;
                PlaySFX(jumpClip);
            }
        }

        if (wallJumpCooldown > 0)
            wallJumpCooldown -= Time.deltaTime;
    }

    void HandleWallSlide()
    {
        if (isTouchingWall && !isGrounded && rb.linearVelocity.y < 0 && GetHorizontal() != 0)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
        }
        else
        {
            isWallSliding = false;
        }
    }

    void FlipSprite(float moveInput)
    {
        if (moveInput > 0) spriteRenderer.flipX = false;
        else if (moveInput < 0) spriteRenderer.flipX = true;
    }

    void SetGravity()
    {
        rb.gravityScale = rb.linearVelocity.y < 0 ? 4f : 3f;
    }

    // ---------------- ATTACK ----------------

    void StartAttack()
    {
        isAttacking = true;
        attackTimer = attackDuration;
        animator.enabled = false;

        if (attackSprite != null)
            spriteRenderer.sprite = attackSprite;
    }

    void HandleAttack()
    {
        if (AttackPressed() && !isAttacking && !isInHitstun)
            StartAttack();

        if (!isAttacking) return;

        attackTimer -= Time.deltaTime;

        if (attackTimer > attackDuration * 0.75f)
        {
            if (attackSprite != null)
                spriteRenderer.sprite = attackSprite;
        }
        else
        {
            if (attackSprite2 != null)
                spriteRenderer.sprite = attackSprite2;
        }

        if (attackTimer <= 0)
        {
            isAttacking = false;
            animator.enabled = true;

            if (normalSprite != null)
                spriteRenderer.sprite = normalSprite;
        }
    }

    // UPDATED: TakeDamage method using damagePercent
    public void TakeDamage(int damage, Vector2 knockbackDirection, float baseKnockbackStrength)
    {
        // Increase damage percentage
        damagePercent += damage;
        damagePercent = Mathf.Clamp(damagePercent, 0, maxDamagePercent);

        // Update health bar
        if (healthBar != null)
        {
            healthBar.SetHealth(damagePercent);
        }

        // Calculate knockback based on damage percentage
        float knockbackMultiplier = 1f + (damagePercent / 100f);
        Vector2 finalKnockback = knockbackDirection * baseKnockbackStrength * knockbackMultiplier;

        // Apply knockback
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(finalKnockback, ForceMode2D.Impulse);

        // Apply hitstun
        isInHitstun = true;
        hitstunTimer = hitstunDuration;

        // Play hurt animation
        if (animator.enabled)
            animator.SetTrigger("Hurt");
    }

    void HandleTimers()
    {
        if (isInHitstun)
        {
            hitstunTimer -= Time.deltaTime;
            if (hitstunTimer <= 0)
                isInHitstun = false;
        }
    }

    // ---------------- PLATFORM DROP ----------------

    void HandlePlatformDrop()
    {
        if (DownPressed() && isPlatform && !canPassThroughPlatform)
            StartPassThroughPlatform();

        if (canPassThroughPlatform)
        {
            passThroughTimer -= Time.deltaTime;
            if (passThroughTimer <= 0)
                StopPassThroughPlatform();
        }
    }

    void StartPassThroughPlatform()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(platformCheck.position, platformCheckRadius, platformLayer);

        foreach (var col in cols)
        {
            if (col.CompareTag("OneWayPlatform"))
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), col, true);
                currentPlatformCollider = col;
                canPassThroughPlatform = true;
                passThroughTimer = passThroughDuration;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -2f);
                break;
            }
        }
    }

    void StopPassThroughPlatform()
    {
        if (currentPlatformCollider != null)
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), currentPlatformCollider, false);
            currentPlatformCollider = null;
        }
        canPassThroughPlatform = false;
    }

    // ---------------- ANIMATION ----------------

    void SetAnimation(float moveInput)
    {
        if (isAttacking || isInHitstun) return;

        if (isGrounded || isPlatform)
        {
            animator.Play(moveInput == 0 ? "Player_Idle" : "Player_Run");
        }
        else
        {
            if (isWallSliding)
                animator.Play("Player_WallSlide");
            else if (rb.linearVelocity.y > 0)
                animator.Play("Player_Jump");
            else
                animator.Play("Player_Fall");
        }
    }

    // ---------------- DAMAGE / DEATH ----------------

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Damage"))
        {
            damagePercent -= 10; // FIXED: Use damagePercent instead of health
            damagePercent = Mathf.Clamp(damagePercent, 0, maxDamagePercent);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            StartCoroutine(FlashRed());

            // Update health bar
            if (healthBar != null)
            {
                healthBar.SetHealth(damagePercent);
            }
        }
    }

    IEnumerator FlashRed()
    {
        Color original = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = original;
    }

    void Die()
    {
        lives--;

        if (lives > 0)
        {
            // Reset damage percentage when respawning
            damagePercent = 0f;
            if (healthBar != null)
            {
                healthBar.SetHealth(damagePercent);
            }

            // Respawn at checkpoint
            if (respawnPoint != null)
            {
                transform.position = respawnPoint.position;
                rb.linearVelocity = Vector2.zero;
            }
        }

        if (lives <= 0)
        {
            Lose();
        }
    }

    void Lose()
    {
        if (hasLost) return; // Only run once
        hasLost = true;

        FindAnyObjectByType<MusicController>()?.StopMusic();
        Time.timeScale = 0f;
        WinUI.SetActive(true);
        PlaySFX(winClip);
    }

    // ---------------- AUDIO ----------------

    void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}