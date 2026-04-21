using System.Collections;
using System.Collections.Generic;
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
    private HashSet<Player> hitPlayersThisAttack;
    public int attackDamage = 10;
    public float baseKnockbackStrength = 8f;
    public float knockbackMultiplier = 2f;

    public float hitstunDuration = 0.3f;
    private float hitstunTimer;
    private bool isInHitstun;

    [Header("Gun")]
    public Projectile projectileprefab;
    public Transform LaunchOffset;
    private Vector2 direction;
    [SerializeField] private float projectilemovespeed;
    private float gunCooldownTimer = 0f;
    public float gunCooldownTime = 10f;

    [Header("References")]
    public Transform respawnPoint;
    public Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private Collider2D playerCollider;

    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite attackSprite;
    public Sprite attackSprite2;
    public Sprite gunSprite;

    [Header("Audio")]
    public AudioClip jumpClip;
    public AudioClip winClip;
    public AudioClip punchClip;
    public AudioClip hurtClip;
    public AudioClip gunShot;

    private bool hasLost = false;

    void Start()
    {
        Application.targetFrameRate = 60;

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        playerCollider = GetComponent<Collider2D>();

        hitPlayersThisAttack = new HashSet<Player>();

        extraJumps = extraJumpsValue;

        if (healthBar == null)
        {
            healthBar = FindFirstObjectByType<HealthBar>();
        }

        if (healthBar != null)
        {
            healthBar.SetHealth(damagePercent);
        }

        if (normalSprite != null)
            spriteRenderer.sprite = normalSprite;
    }

    void Update()
    {
        float moveInput = GetHorizontal();

        HandleTimers();
        HandlePlatformDrop();
        HandleGunShot();

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

        if (transform.position.y < -9.5)
            Die();
        else if (transform.position.y > 8)
            Die();
        else if (transform.position.x > 15)
            Die();
        else if (transform.position.x < -15)
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
    bool GunShot()
    {
        return playerIndex == 1
            ? Input.GetKeyDown(KeyCode.H)
            : Input.GetKeyDown(KeyCode.Space);
    }

    void HandleJump()
    {
        if (isGrounded || isPlatform)
        {
            extraJumps = extraJumpsValue;
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

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
        if (isTouchingWall && !isGrounded && rb.linearVelocityY < 0.15f && wallJumpCooldown <= 0f)
        {
            isWallSliding = true;
            extraJumps = extraJumpsValue;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
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

    void StartAttack()
    {
        isAttacking = true;
        attackTimer = attackDuration;
        hitPlayersThisAttack.Clear();
        animator.enabled = false;
        PlaySFX(punchClip);

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

    void HandleGunShot()
    {
        if (gunCooldownTimer > 0f)
        {
            Debug.Log($"Gun on cooldown! {gunCooldownTimer:F1} seconds remaining");
            return;
        }

        if (GunShot() && !isAttacking && !isInHitstun)
        {
            Vector2 shootDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
            Projectile projectile = Instantiate(projectileprefab, LaunchOffset.position, Quaternion.identity);
            projectile.InitializeProjectile(shootDirection, projectilemovespeed, this);
            PlaySFX(gunShot);

            gunCooldownTimer = gunCooldownTime;
        }
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection, float baseKnockbackStrength)
    {
        damagePercent += damage;
        damagePercent = Mathf.Clamp(damagePercent, 0, maxDamagePercent);

        if (healthBar != null)
        {
            healthBar.SetHealth(damagePercent);
        }

        float finalKnockbackStrength = (baseKnockbackStrength + (damagePercent / 20f)) * knockbackMultiplier;

        Vector2 finalKnockback = knockbackDirection * finalKnockbackStrength;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(finalKnockback, ForceMode2D.Impulse);

        isInHitstun = true;
        hitstunTimer = hitstunDuration;

        PlaySFX(hurtClip);
    }

    private bool HasAnimatorParameter(string paramName)
    {
        if (animator == null) return false;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    void HandleTimers()
    {
        if (isInHitstun)
        {
            hitstunTimer -= Time.deltaTime;
            if (hitstunTimer <= 0)
                isInHitstun = false;
        }

        if (gunCooldownTimer > 0f)
        {
            gunCooldownTimer -= Time.deltaTime;
            if (gunCooldownTimer < 0f)
                gunCooldownTimer = 0f;
        }
    }

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
                Physics2D.IgnoreCollision(playerCollider, col, true);
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
            Physics2D.IgnoreCollision(playerCollider, currentPlatformCollider, false);
            currentPlatformCollider = null;
        }
        canPassThroughPlatform = false;
    }

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

    private void OnCollisionStay2D(Collision2D collision)
    {
        Player other = collision.gameObject.GetComponent<Player>();

        FlashRed();

        if (other != null && other != this && isAttacking && !hitPlayersThisAttack.Contains(other))
        {
            hitPlayersThisAttack.Add(other);

            Vector2 direction = (other.transform.position - transform.position).normalized;

            other.TakeDamage(attackDamage, direction, baseKnockbackStrength);
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
            damagePercent = 0f;
            if (healthBar != null)
            {
                healthBar.SetHealth(damagePercent);
            }

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
        if (hasLost) return;
        hasLost = true;

        FindAnyObjectByType<MusicController>()?.StopMusic();
        Time.timeScale = 0f;

        if (WinUI != null)
            WinUI.SetActive(true);

        PlaySFX(winClip);
    }

    void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}