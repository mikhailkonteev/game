using UnityEngine;

public class FighterAnimator : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb;
    public PlayerCombat combat;

    [Header("Frames")]
    public Sprite[] idleFrames;
    public Sprite[] runFrames;
    public Sprite[] jumpFrames;
    public Sprite[] fallFrames;
    public Sprite[] attackFrames;
    public Sprite[] heavyAttackFrames;

    [Header("Timing")]
    public float idleFrameRate = 8f;
    public float runFrameRate = 12f;
    public float airFrameRate = 10f;
    public float attackFrameRate = 16f;

    private Sprite[] currentFrames;
    private int frameIndex;
    private float frameTimer;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (combat == null)
            combat = GetComponent<PlayerCombat>();
    }

    void Update()
    {
        if (GameManager.IsGameOver)
            return;

        Sprite[] nextFrames = GetFramesForState();
        float frameRate = GetFrameRate(nextFrames);
        Play(nextFrames, frameRate);
    }

    Sprite[] GetFramesForState()
    {
        Vector2 velocity = rb.linearVelocity;

        if (combat != null && combat.IsHeavyAttacking && HasFrames(heavyAttackFrames))
            return heavyAttackFrames;

        if (combat != null && combat.IsAttacking && HasFrames(attackFrames))
            return attackFrames;

        if (velocity.y > 0.1f && HasFrames(jumpFrames))
            return jumpFrames;

        if (velocity.y < -0.1f && HasFrames(fallFrames))
            return fallFrames;

        if (Mathf.Abs(velocity.x) > 0.1f && HasFrames(runFrames))
            return runFrames;

        return idleFrames;
    }

    bool HasFrames(Sprite[] frames)
    {
        return frames != null && frames.Length > 0;
    }

    float GetFrameRate(Sprite[] frames)
    {
        if (frames == attackFrames || frames == heavyAttackFrames)
            return attackFrameRate;

        if (frames == runFrames)
            return runFrameRate;

        if (frames == jumpFrames || frames == fallFrames)
            return airFrameRate;

        return idleFrameRate;
    }

    void Play(Sprite[] frames, float frameRate)
    {
        if (frames == null || frames.Length == 0)
            return;

        if (currentFrames != frames)
        {
            currentFrames = frames;
            frameIndex = 0;
            frameTimer = 0f;
            spriteRenderer.sprite = currentFrames[frameIndex];
        }

        frameTimer += Time.deltaTime;

        if (frameTimer < 1f / frameRate)
            return;

        frameTimer = 0f;
        frameIndex = (frameIndex + 1) % currentFrames.Length;
        spriteRenderer.sprite = currentFrames[frameIndex];
    }
}
