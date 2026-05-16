using UnityEngine;

public class PlayerDamage : MonoBehaviour
{
    [Header("Damage")]
    public float damage = 50f;

    [Header("Raycast")]
    public Transform attackPoint;
    public float attackRange = 1.5f;
    public float attackRadius = 0.5f;
    public LayerMask enemyLayer;

    public void DoDamage()
    {
        float finalDamage = PlayerStats.Instance != null
            ? PlayerStats.Instance.strength
            : damage;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            attackPoint.position,
            attackRadius,
            GetAttackDirection(),
            attackRange,
            enemyLayer
        );

        foreach (RaycastHit2D hit in hits)
        {
            IDamageable target = hit.collider.GetComponent<IDamageable>();

            if (target != null)
            {
                target.TakeDamage(finalDamage);
                if (DamagePopupPool.Instance != null)
                {
                    DamagePopupPool.Instance.ShowDamage((int)finalDamage, transform.position);
                }
                Debug.Log($"💥 Hit enemy: {hit.collider.name}");
            }
        }
    }

    private Vector2 GetAttackDirection()
    {
        // Nếu nhân vật quay bằng scale X
        return transform.localScale.x >= 0 ? Vector2.right : Vector2.left;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);

        Vector2 dir = Application.isPlaying ? GetAttackDirection() : Vector2.right;
        Gizmos.DrawWireSphere(
            (Vector2)attackPoint.position + dir * attackRange,
            attackRadius
        );
    }
}