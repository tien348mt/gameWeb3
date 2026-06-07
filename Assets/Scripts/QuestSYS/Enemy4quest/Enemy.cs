using System.Collections;
using UnityEngine;

/// <summary>
/// Enemy dùng cho quest. Set stats qua Inspector, báo QuestManager khi chết.
/// </summary>
public class QuestEnemy : BaseEnemy
{
    [Header("=== Quest ===")]
    public string enemyID;       

    [Header("=== Stats ===")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float attackDamage = 12f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private float defense = 10f;

    // ── Override stats ───────────────────────────────────────
    protected override int MaxHealth => maxHealth;
    protected override float AttackDamage => attackDamage;
    protected override float AttackCooldown => attackCooldown;
    protected override float Defense => defense;
    IEnumerator Start()
    {
        yield return new WaitUntil(() => QuestManager.Instance != null
                                       && QuestManager.Instance.IsLoaded);

        if (QuestManager.Instance.IsDestroyed(enemyID))
            Destroy(gameObject);
    }
    public override string GetKillID() => enemyID;

    // ── Khi chết: thông báo quest ────────────────────────────
    protected override void OnDeath()
    {
        Debug.Log($"☠️ QuestEnemy chết: {enemyID}");
        QuestManager.Instance?.NotifyEnemyKilled(enemyID);
    }
    public void SlashSFX()
    {
        AudioManager.Instance.PlaySFX(2);
    }
}