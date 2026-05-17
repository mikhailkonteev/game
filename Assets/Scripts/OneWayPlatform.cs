using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    public float dropDuration = 0.6f;

    void Awake()
    {
        Collider2D platformCollider = GetComponent<Collider2D>();

        if (platformCollider == null)
            return;

        PlatformEffector2D platformEffector = GetComponent<PlatformEffector2D>();

        if (platformEffector == null)
            platformEffector = gameObject.AddComponent<PlatformEffector2D>();

        platformEffector.useOneWay = true;
        platformCollider.usedByEffector = true;
    }
}
