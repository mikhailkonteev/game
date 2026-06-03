using System.Collections;
using UnityEngine;

public class CombatVfx : MonoBehaviour
{
    private static CombatVfx instance;
    private Camera targetCamera;
    private Vector3 cameraStartPosition;

    public static CombatVfx Ensure()
    {
        if (instance != null)
            return instance;

        GameObject vfxObject = new GameObject("CombatVfx");
        instance = vfxObject.AddComponent<CombatVfx>();
        return instance;
    }

    public static void Hit(Vector3 position, bool heavy)
    {
        Ensure().SpawnHit(position, heavy);
        if (heavy)
            Ensure().ShakeCamera(0.16f, 0.18f);
    }

    public static void Pickup(Vector3 position)
    {
        Ensure().SpawnPickupBurst(position);
    }

    public static void Overheat(Vector3 position)
    {
        Ensure().SpawnOverheatBurst(position);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void SpawnHit(Vector3 position, bool heavy)
    {
        GameObject slash = GameObject.CreatePrimitive(PrimitiveType.Quad);
        slash.name = heavy ? "HeavyHitSlash" : "HitSlash";
        slash.transform.position = new Vector3(position.x, position.y, -0.5f);
        slash.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-28f, 28f));
        slash.transform.localScale = heavy ? new Vector3(1.25f, 0.18f, 1f) : new Vector3(0.85f, 0.12f, 1f);

        Renderer renderer = slash.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.material.color = heavy ? new Color(1f, 0.55f, 0.1f, 0.92f) : new Color(1f, 0.95f, 0.62f, 0.86f);

        Destroy(slash.GetComponent<Collider>());
        StartCoroutine(FadeAndDestroy(renderer, slash.transform, 0.16f, heavy ? 1.35f : 1.18f));
    }

    private void SpawnPickupBurst(Vector3 position)
    {
        for (int i = 0; i < 7; i++)
        {
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Quad);
            spark.name = "PickupSpark";
            spark.transform.position = new Vector3(position.x, position.y, -0.45f);
            spark.transform.localScale = Vector3.one * Random.Range(0.08f, 0.15f);

            Renderer renderer = spark.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.material.color = new Color(0.45f, 0.9f, 1f, 0.9f);

            Destroy(spark.GetComponent<Collider>());
            StartCoroutine(SparkRoutine(spark, renderer, Random.insideUnitCircle.normalized * Random.Range(0.45f, 0.95f), 0.34f));
        }
    }

    private void SpawnOverheatBurst(Vector3 position)
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Quad);
            spark.name = "OverheatSpark";
            spark.transform.position = new Vector3(position.x, position.y, -0.45f);
            spark.transform.localScale = Vector3.one * Random.Range(0.1f, 0.2f);

            Renderer renderer = spark.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.material.color = new Color(1f, Random.Range(0.35f, 0.72f), 0.05f, 0.9f);

            Destroy(spark.GetComponent<Collider>());
            StartCoroutine(SparkRoutine(spark, renderer, Random.insideUnitCircle.normalized * Random.Range(0.55f, 1.1f), 0.42f));
        }
    }

    private IEnumerator FadeAndDestroy(Renderer renderer, Transform target, float duration, float scaleMultiplier)
    {
        float elapsed = 0f;
        Color start = renderer.material.color;
        Vector3 startScale = target.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            renderer.material.color = new Color(start.r, start.g, start.b, Mathf.Lerp(start.a, 0f, t));
            target.localScale = Vector3.Lerp(startScale, startScale * scaleMultiplier, t);
            yield return null;
        }

        Destroy(target.gameObject);
    }

    private IEnumerator SparkRoutine(GameObject spark, Renderer renderer, Vector2 velocity, float duration)
    {
        float elapsed = 0f;
        Color start = renderer.material.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            spark.transform.position += (Vector3)(velocity * Time.deltaTime);
            renderer.material.color = new Color(start.r, start.g, start.b, Mathf.Lerp(start.a, 0f, t));
            yield return null;
        }

        Destroy(spark);
    }

    private void ShakeCamera(float duration, float strength)
    {
        targetCamera = Camera.main;
        if (targetCamera == null)
            return;

        StopCoroutine(nameof(CameraShakeRoutine));
        StartCoroutine(CameraShakeRoutine(duration, strength));
    }

    private IEnumerator CameraShakeRoutine(float duration, float strength)
    {
        cameraStartPosition = targetCamera.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Vector2 offset = Random.insideUnitCircle * strength * (1f - elapsed / duration);
            targetCamera.transform.position = cameraStartPosition + new Vector3(offset.x, offset.y, 0f);
            yield return null;
        }

        if (targetCamera != null)
            targetCamera.transform.position = cameraStartPosition;
    }
}
