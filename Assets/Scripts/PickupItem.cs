using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public enum PickupType
    {
        Health,
        Shield,
        Coolant,
        DamageBoost
    }

    public PickupType type;
    public int healAmount = 30;
    public float shieldDamageMultiplier = 0.5f;
    public float shieldDuration = 10f;
    public float damageBoostMultiplier = 1.3f;
    public float damageBoostDuration = 10f;

    private float spawnedAtTime;

    void Awake()
    {
        spawnedAtTime = Time.time;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Health health = other.GetComponent<Health>();
        PlayerCombat combat = other.GetComponent<PlayerCombat>();

        if (health == null && combat == null)
            return;

        int healthBeforePickup = health != null ? health.currentHealth : -1;
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
            gameManager.RegisterPickup(health, combat, type, healthBeforePickup, Time.time - spawnedAtTime);

        Apply(health, combat);
        AudioManager.PlayBoost();
        CombatVfx.Pickup(transform.position);
        Destroy(gameObject);
    }

    void Apply(Health health, PlayerCombat combat)
    {
        switch (type)
        {
            case PickupType.Health:
                if (health != null)
                    health.Heal(healAmount);
                break;
            case PickupType.Shield:
                if (health != null)
                    health.ApplyDamageReduction(shieldDamageMultiplier, shieldDuration);
                break;
            case PickupType.Coolant:
                if (combat != null)
                    combat.CoolHeat();
                break;
            case PickupType.DamageBoost:
                if (combat != null)
                    combat.ApplyDamageBoost(damageBoostMultiplier, damageBoostDuration);
                break;
        }
    }
}
