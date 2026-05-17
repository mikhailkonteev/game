using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public KeyCode attackKey;
    public Transform attackPoint;
    public float attackRange = 1f;
    public int attackDamage = 10;
    public LayerMask enemyLayer;

    void Update()
    {
        if (GameManager.IsGameOver)
            return;

        if (Input.GetKeyDown(attackKey))
        {
            Attack();
        }
    }

    void Attack()
    {
        Collider2D hitEnemy = Physics2D.OverlapCircle(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemy != null)
        {
            Health enemyHealth = hitEnemy.GetComponent<Health>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
