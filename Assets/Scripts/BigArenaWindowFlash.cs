using UnityEngine;

public class BigArenaWindowFlash : MonoBehaviour
{
    public Sprite flashBackgroundSprite;
    public float minFlashInterval = 15f;
    public float maxFlashInterval = 25f;
    public float flashDuration = 0.6f;

    private SpriteRenderer backgroundRenderer;
    private Sprite normalBackgroundSprite;
    private float nextFlashTime;
    private float restoreAtTime;
    private bool isFlashing;

    void Start()
    {
        backgroundRenderer = GetComponent<SpriteRenderer>();
        if (backgroundRenderer == null || flashBackgroundSprite == null)
        {
            enabled = false;
            return;
        }

        normalBackgroundSprite = backgroundRenderer.sprite;
        ScheduleNextFlash();
    }

    void Update()
    {
        if (GameManager.IsPaused || backgroundRenderer == null)
            return;

        if (!isFlashing && Time.time >= nextFlashTime)
        {
            isFlashing = true;
            backgroundRenderer.sprite = flashBackgroundSprite;
            restoreAtTime = Time.time + flashDuration;
        }

        if (isFlashing && Time.time >= restoreAtTime)
        {
            isFlashing = false;
            backgroundRenderer.sprite = normalBackgroundSprite;
            ScheduleNextFlash();
        }
    }

    void ScheduleNextFlash()
    {
        nextFlashTime = Time.time + Random.Range(minFlashInterval, maxFlashInterval);
    }
}
