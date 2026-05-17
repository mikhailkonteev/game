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

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private Collider2D currentOneWayPlatform;
    private bool isGrounded;
    private bool wasGrounded;
    private bool facingRight = true;
    private int jumpsRemaining;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        facingRight = transform.localScale.x > 0f;
        jumpsRemaining = maxJumps;
    }

    void Update()
    {
        if (GameManager.IsInputBlocked)
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

        if (Input.GetKey(leftKey))
            moveInput = -1f;
        else if (Input.GetKey(rightKey))
            moveInput = 1f;

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

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
        if (isGrounded && !wasGrounded)
        {
            jumpsRemaining = maxJumps;
        }

        if (Input.GetKeyDown(jumpKey) && jumpsRemaining > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpsRemaining--;
        }

        wasGrounded = isGrounded;
    }

    void UpdateGrounded()
    {
        isGrounded = currentOneWayPlatform != null;

        if (groundCheck != null)
        {
            isGrounded |= Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
    }

    void DropThroughPlatform()
    {
        if (!Input.GetKeyDown(downKey))
            return;

        if (currentOneWayPlatform != null && currentOneWayPlatform.TryGetComponent(out OneWayPlatform currentPlatform))
        {
            StartCoroutine(TemporarilyIgnorePlatform(currentOneWayPlatform, currentPlatform.dropDuration));
            currentOneWayPlatform = null;
            return;
        }

        if (groundCheck == null)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent(out OneWayPlatform platform))
            {
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
} 
