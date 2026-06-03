using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public int maxJumps = 2;

    [Header("Controls")]
    public KeyCode leftKey;
    public KeyCode rightKey;
    public KeyCode jumpKey;
    public KeyCode downKey;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("One-way Platforms")]
    public float platformDropDuration = 0.6f;
    public float platformDropVelocity = -4f;
    public float jumpGroundLockout = 0.14f;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private PlayerCombat combat;
    private Collider2D currentOneWayPlatform;
    private bool isGrounded;
    private bool facingRight = true;
    private int jumpsUsed;
    private float ignoreGroundUntil;
    private bool usePlayer1Bindings;
    private bool usePlayer2Bindings;

    public bool HasUsedDoubleJump => jumpsUsed >= 2 && !isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        combat = GetComponent<PlayerCombat>();
        facingRight = transform.localScale.x > 0f;
        jumpsUsed = 0;
        usePlayer1Bindings = leftKey == KeyCode.A || jumpKey == KeyCode.W;
        usePlayer2Bindings = leftKey == KeyCode.LeftArrow || jumpKey == KeyCode.UpArrow;
    }

    void Update()
    {
        if (GameManager.IsInputBlocked)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (combat != null && combat.IsOverheated)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Move();
        UpdateGrounded();
        DropThroughPlatform();
        Jump();
    }

    void Move()
    {
        float moveInput = 0f;

        if (Input.GetKey(GetLeftKey()))
            moveInput = -1f;
        else if (Input.GetKey(GetRightKey()))
            moveInput = 1f;

        float speedMultiplier = combat != null ? combat.MovementSpeedMultiplier : 1f;
        rb.linearVelocity = new Vector2(moveInput * moveSpeed * speedMultiplier, rb.linearVelocity.y);

        if (moveInput < 0 && facingRight)
        {
            Flip();
        }
        else if (moveInput > 0 && !facingRight)
        {
            Flip();
        }
    }

    void Jump()
    {
        if (isGrounded && rb.linearVelocity.y <= 0.05f)
        {
            jumpsUsed = 0;
        }

        if (Input.GetKeyDown(GetJumpKey()) && jumpsUsed < maxJumps)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpsUsed++;
            isGrounded = false;
            currentOneWayPlatform = null;
            ignoreGroundUntil = Time.time + jumpGroundLockout;
            AudioManager.PlayJump();
        }
    }

    void UpdateGrounded()
    {
        if (Time.time < ignoreGroundUntil)
        {
            isGrounded = false;
            return;
        }

        isGrounded = currentOneWayPlatform != null;

        if (groundCheck != null)
        {
            isGrounded |= Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
    }

    void DropThroughPlatform()
    {
        if (!Input.GetKeyDown(GetDownKey()))
            return;

        if (currentOneWayPlatform != null && currentOneWayPlatform.TryGetComponent(out OneWayPlatform currentPlatform))
        {
            RegisterPlatformDrop();
            StartCoroutine(TemporarilyIgnorePlatform(currentOneWayPlatform, currentPlatform.dropDuration));
            currentOneWayPlatform = null;
            isGrounded = false;
            return;
        }

        if (groundCheck == null)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent(out OneWayPlatform platform))
            {
                RegisterPlatformDrop();
                StartCoroutine(TemporarilyIgnorePlatform(hit, platform.dropDuration));
                break;
            }
        }
    }

    IEnumerator TemporarilyIgnorePlatform(Collider2D platformCollider, float duration)
    {
        float ignoreDuration = duration > 0f ? duration : platformDropDuration;

        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, platformDropVelocity);

        yield return new WaitForSeconds(ignoreDuration);

        if (playerCollider != null && platformCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
        }
    }

    public void ResetRoundState()
    {
        StopAllCoroutines();
        currentOneWayPlatform = null;
        isGrounded = false;
        jumpsUsed = 0;
        ignoreGroundUntil = 0f;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.collider.TryGetComponent(out OneWayPlatform _))
            return;

        currentOneWayPlatform = collision.collider;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider == currentOneWayPlatform)
        {
            currentOneWayPlatform = null;
        }
    }

    void Flip()
    {
        facingRight = !facingRight;

        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    KeyCode GetLeftKey()
    {
        if (usePlayer1Bindings)
            return ControlBindings.Get(ControlAction.Player1Left);

        if (usePlayer2Bindings)
            return ControlBindings.Get(ControlAction.Player2Left);

        return leftKey;
    }

    KeyCode GetRightKey()
    {
        if (usePlayer1Bindings)
            return ControlBindings.Get(ControlAction.Player1Right);

        if (usePlayer2Bindings)
            return ControlBindings.Get(ControlAction.Player2Right);

        return rightKey;
    }

    KeyCode GetJumpKey()
    {
        if (usePlayer1Bindings)
            return ControlBindings.Get(ControlAction.Player1Jump);

        if (usePlayer2Bindings)
            return ControlBindings.Get(ControlAction.Player2Jump);

        return jumpKey;
    }

    KeyCode GetDownKey()
    {
        if (usePlayer1Bindings)
            return ControlBindings.Get(ControlAction.Player1Drop);

        if (usePlayer2Bindings)
            return ControlBindings.Get(ControlAction.Player2Drop);

        return downKey;
    }

    void RegisterPlatformDrop()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
            gameManager.RegisterPlatformDrop(this);
    }
} 
