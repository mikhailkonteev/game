using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public KeyCode attackKey;
    public KeyCode heavyAttackKey;
    public Transform attackPoint;
    public float attackRange = 1f;
    public int attackDamage = 10;
    public float attackAnimationDuration = 0.35f;
    public float heavyAttackRange = 1.35f;
    public int heavyAttackDamage = 18;
    public float heavyAttackAnimationDuration = 0.55f;
    [Range(0f, 1f)]
    public float damageVariance = 0.3f;

    [Header("Heat")]
    public float maxHeat = 100f;
    public float attackHeat = 10f;
    public float heavyAttackHeat = 20f;
    public float overheatResetHeat = 55f;
    public float heatDecayPerSecond = 3f;
    public float overheatFreezeDuration = 2f;
    public float overheatSlowDuration = 5f;
    [Range(0f, 1f)]
    public float overheatSlowMultiplier = 0.55f;

    public LayerMask enemyLayer;

    private float attackAnimationEndTime;
    private float currentHeat;
    private float freezeEndTime;
    private float slowEndTime;
    private float damageBoostMultiplier = 1f;
    private float damageBoostEndTime;
    private bool isHeavyAttacking;
    private bool isOverheated;
    private bool usePlayer1Bindings;
    private bool usePlayer2Bindings;
    private bool heatWarningPlayed;

    public bool IsAttacking => Time.time < attackAnimationEndTime;
    public bool IsHeavyAttacking => IsAttacking && isHeavyAttacking;
    public bool IsOverheated => isOverheated && Time.time < freezeEndTime;
    public bool HasDamageBoost => damageBoostMultiplier > 1f && Time.time < damageBoostEndTime;
    public float HeatPercent => maxHeat > 0f ? Mathf.Clamp01(currentHeat / maxHeat) : 0f;
    public bool IsHeatCritical => HeatPercent >= 0.85f;
    public float MovementSpeedMultiplier => Time.time < slowEndTime ? overheatSlowMultiplier : 1f;

    void Start()
    {
        usePlayer1Bindings = attackKey == KeyCode.F;
        usePlayer2Bindings = attackKey == KeyCode.K;
    }

    void Update()
    {
        UpdateHeat();

        if (GameManager.IsInputBlocked)
            return;

        if (IsOverheated)
            return;

        if (IsAttacking)
            return;

        if (IsHeavyAttackPressed())
        {
            StartAttack(heavyAttackAnimationDuration, heavyAttackDamage, heavyAttackRange, heavyAttackHeat, true);
        }
        else if (Input.GetKeyDown(GetLightAttackKey()))
        {
            StartAttack(attackAnimationDuration, attackDamage, attackRange, attackHeat, false);
        }
    }

    void UpdateHeat()
    {
        if (GameManager.IsPaused)
            return;

        if (currentHeat > 0f)
            currentHeat = Mathf.Max(0f, currentHeat - heatDecayPerSecond * Time.deltaTime);

        if (HeatPercent < 0.72f)
            heatWarningPlayed = false;

        if (isOverheated && Time.time >= slowEndTime)
            isOverheated = false;

        if (damageBoostMultiplier > 1f && Time.time >= damageBoostEndTime)
            damageBoostMultiplier = 1f;
    }

    bool IsHeavyAttackPressed()
    {
        KeyCode boundHeavyAttackKey = GetHeavyAttackKey();
        if (boundHeavyAttackKey != KeyCode.None && Input.GetKeyDown(boundHeavyAttackKey))
            return true;

        return false;
    }

    KeyCode GetLightAttackKey()
    {
        if (usePlayer1Bindings)
            return ControlBindings.Get(ControlAction.Player1LightAttack);

        if (usePlayer2Bindings)
            return ControlBindings.Get(ControlAction.Player2LightAttack);

        return attackKey;
    }

    KeyCode GetHeavyAttackKey()
    {
        if (usePlayer1Bindings)
            return ControlBindings.Get(ControlAction.Player1HeavyAttack);

        if (usePlayer2Bindings)
            return ControlBindings.Get(ControlAction.Player2HeavyAttack);

        return heavyAttackKey;
    }

    void StartAttack(float animationDuration, int damage, float range, float heatIncrease, bool isHeavy)
    {
        attackAnimationEndTime = Time.time + animationDuration;
        isHeavyAttacking = isHeavy;
        if (isHeavy)
            AudioManager.PlayHeavyAttack();
        else
            AudioManager.PlayLightAttack();
        AddHeat(heatIncrease);
        Attack(damage, range, isHeavy);
    }

    void AddHeat(float heatIncrease)
    {
        if (maxHeat <= 0f || isOverheated)
            return;

        currentHeat = Mathf.Clamp(currentHeat + heatIncrease, 0f, maxHeat);

        if (!heatWarningPlayed && currentHeat < maxHeat && HeatPercent >= 0.85f)
        {
            heatWarningPlayed = true;
            AudioManager.PlayOverheatWarning();
        }

        if (currentHeat >= maxHeat)
            TriggerOverheat();
    }

    void TriggerOverheat()
    {
        isOverheated = true;
        isHeavyAttacking = false;
        currentHeat = Mathf.Clamp(overheatResetHeat, 0f, maxHeat);
        freezeEndTime = Time.time + overheatFreezeDuration;
        slowEndTime = freezeEndTime + overheatSlowDuration;
        attackAnimationEndTime = freezeEndTime;
        heatWarningPlayed = false;
        AudioManager.PlayOverheat();
        CombatVfx.Overheat(transform.position);

        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
            gameManager.RegisterOverheat(this);
    }

    void Attack(int damage, float range, bool isHeavy)
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, range, enemyLayer);

        if (hitEnemy != null)
        {
            Health enemyHealth = hitEnemy.GetComponent<Health>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(GetBoostedDamage(GetRandomizedDamage(damage)));
                AudioManager.PlayHit();
                CombatVfx.Hit(hitEnemy.transform.position, isHeavy);
                GameManager gameManager = FindObjectOfType<GameManager>();
                PlayerController controller = GetComponent<PlayerController>();
                if (gameManager != null)
                    gameManager.RegisterAttackHit(this, isHeavy, controller != null && controller.HasUsedDoubleJump, enemyHealth.currentHealth <= 0);
            }
        }
    }

    public void CoolHeat()
    {
        currentHeat = 0f;
    }

    public void ApplyDamageBoost(float multiplier, float duration)
    {
        damageBoostMultiplier = Mathf.Max(1f, multiplier);
        damageBoostEndTime = Time.time + duration;
    }

    public void ResetCombatState()
    {
        attackAnimationEndTime = 0f;
        currentHeat = 0f;
        freezeEndTime = 0f;
        slowEndTime = 0f;
        damageBoostMultiplier = 1f;
        damageBoostEndTime = 0f;
        heatWarningPlayed = false;
        isHeavyAttacking = false;
        isOverheated = false;
    }

    int GetBoostedDamage(int damage)
    {
        return Mathf.Max(1, Mathf.RoundToInt(damage * damageBoostMultiplier));
    }

    int GetRandomizedDamage(int baseDamage)
    {
        float variance = Mathf.Clamp01(damageVariance);
        float minDamage = baseDamage * (1f - variance);
        float maxDamage = baseDamage * (1f + variance);
        return Mathf.Max(1, Mathf.RoundToInt(Random.Range(minDamage, maxDamage)));
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, heavyAttackRange);
    }
}
