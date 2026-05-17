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
    public LayerMask enemyLayer;

    private float attackAnimationEndTime;
    private bool isHeavyAttacking;

    public bool IsAttacking => Time.time < attackAnimationEndTime;
    public bool IsHeavyAttacking => IsAttacking && isHeavyAttacking;

    void Update()
    {
        if (GameManager.IsInputBlocked)
            return;

        if (IsAttacking)
            return;

        if (IsHeavyAttackPressed())
        {
            StartAttack(heavyAttackAnimationDuration, heavyAttackDamage, heavyAttackRange, true);
        }
        else if (Input.GetKeyDown(attackKey))
        {
            StartAttack(attackAnimationDuration, attackDamage, attackRange, false);
        }
    }

    bool IsHeavyAttackPressed()
    {
        if (heavyAttackKey != KeyCode.None && Input.GetKeyDown(heavyAttackKey))
            return true;

        KeyCode fallbackHeavyAttackKey = GetFallbackHeavyAttackKey();
        return fallbackHeavyAttackKey != KeyCode.None && Input.GetKeyDown(fallbackHeavyAttackKey);
    }

    KeyCode GetFallbackHeavyAttackKey()
    {
        if (attackKey == KeyCode.F)
            return KeyCode.G;

        if (attackKey == KeyCode.K)
            return KeyCode.L;

        return KeyCode.None;
    }

    void StartAttack(float animationDuration, int damage, float range, bool isHeavy)
    {
        attackAnimationEndTime = Time.time + animationDuration;
        isHeavyAttacking = isHeavy;
        Attack(damage, range);
    }

    void Attack(int damage, float range)
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, range, enemyLayer);

        if (hitEnemy != null)
        {
            Health enemyHealth = hitEnemy.GetComponent<Health>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(GetRandomizedDamage(damage));
            }
        }
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
