using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    public float minSpawnInterval = 8f;
    public float maxSpawnInterval = 15f;
    public float spawnXMin = -6.5f;
    public float spawnXMax = 6.5f;
    public float spawnY = 2.1f;
    public float pickupScale = 1.2f;

    private readonly string[] resourceNames = { "Boosts/health", "Boosts/shield", "Boosts/coolant", "Boosts/damage" };
    private Sprite[] sprites;
    private GameObject currentPickup;
    private float nextSpawnTime;

    void Awake()
    {
        sprites = new Sprite[resourceNames.Length];

        for (int i = 0; i < resourceNames.Length; i++)
            sprites[i] = Resources.Load<Sprite>(resourceNames[i]);
    }

    void Start()
    {
        ScheduleNextSpawn();
    }

    void Update()
    {
        if (GameManager.IsInputBlocked)
            return;

        if (Time.time >= nextSpawnTime)
            SpawnPickup();
    }

    void SpawnPickup()
    {
        if (currentPickup != null)
            Destroy(currentPickup);

        int index = Random.Range(0, resourceNames.Length);
        Vector3 spawnPosition = new Vector3(Random.Range(spawnXMin, spawnXMax), spawnY, 0f);

        currentPickup = new GameObject("Pickup_" + ((PickupItem.PickupType)index));
        currentPickup.transform.position = spawnPosition;
        currentPickup.transform.localScale = Vector3.one * pickupScale;

        SpriteRenderer spriteRenderer = currentPickup.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprites[index];
        spriteRenderer.sortingOrder = 10;

        CircleCollider2D collider = currentPickup.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.45f;

        PickupItem pickupItem = currentPickup.AddComponent<PickupItem>();
        pickupItem.type = (PickupItem.PickupType)index;

        ScheduleNextSpawn();
    }

    void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
    }
}
