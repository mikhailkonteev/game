using UnityEngine;

public class LargeArenaBuilder : MonoBehaviour
{
    private const int PlatformLayer = 6;
    public Sprite platformSprite;
    public float platformHeight = 0.16f;

    void Awake()
    {
        CreatePlatform("UpperLeftPlatform", -18.6f, 7.0f, 11.5f);
        CreatePlatform("UpperRightPlatform", 18.6f, 7.0f, 11.5f);
        CreatePlatform("MiddlePlatform", 0f, 0.5f, 21.0f);
        CreatePlatform("LowerLeftPlatform", -18.6f, -6.0f, 11.5f);
        CreatePlatform("LowerRightPlatform", 18.6f, -6.0f, 11.5f);
    }

    void CreatePlatform(string platformName, float x, float y, float width)
    {
        GameObject platform = new GameObject(platformName);
        platform.layer = PlatformLayer;
        platform.transform.SetParent(transform);
        platform.transform.position = new Vector3(x, y, 0f);
        platform.transform.localScale = new Vector3(width, platformHeight, 1f);

        SpriteRenderer renderer = platform.AddComponent<SpriteRenderer>();
        renderer.sprite = GetPlatformSprite();
        renderer.color = Color.white;
        renderer.sortingOrder = 3;

        BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
        collider.usedByEffector = true;

        PlatformEffector2D effector = platform.AddComponent<PlatformEffector2D>();
        effector.useOneWay = true;

        OneWayPlatform oneWayPlatform = platform.AddComponent<OneWayPlatform>();
        oneWayPlatform.dropDuration = 0.45f;
    }

    Sprite GetPlatformSprite()
    {
        if (platformSprite != null)
            return platformSprite;

        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        texture.hideFlags = HideFlags.HideAndDontSave;

        platformSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, 1f, 1f),
            new Vector2(0.5f, 0.5f),
            1f);
        platformSprite.hideFlags = HideFlags.HideAndDontSave;

        return platformSprite;
    }
}
