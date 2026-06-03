using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public float damageFlashDuration = 0.2f;

    private float damageTakenMultiplier = 1f;
    private float damageReductionEndTime;
    private float damageFlashEndTime;

    public bool IsDamageFlashing => Time.time < damageFlashEndTime;
    public bool HasDamageReduction => damageTakenMultiplier < 1f && Time.time < damageReductionEndTime;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (damageTakenMultiplier < 1f && Time.time >= damageReductionEndTime)
            damageTakenMultiplier = 1f;
    }

    public void TakeDamage(int damage)
    {
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(damage * damageTakenMultiplier));
        int previousHealth = currentHealth;
        currentHealth -= finalDamage;

        if (currentHealth < 0)
            currentHealth = 0;

        damageFlashEndTime = Time.time + damageFlashDuration;

        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.RegisterDamage(this, previousHealth - currentHealth);

            if (damageTakenMultiplier < 1f)
                gameManager.RegisterShieldBlock(this);
        }

        Debug.Log(gameObject.name + " HP: " + currentHealth);
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    public void ApplyDamageReduction(float multiplier, float duration)
    {
        damageTakenMultiplier = Mathf.Clamp01(multiplier);
        damageReductionEndTime = Time.time + duration;
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        damageTakenMultiplier = 1f;
        damageReductionEndTime = 0f;
        damageFlashEndTime = 0f;
    }
}
