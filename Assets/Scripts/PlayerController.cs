using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Controls")]
    public KeyCode leftKey;
    public KeyCode rightKey;
    public KeyCode jumpKey;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (GameManager.IsGameOver)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Move();
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
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
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
