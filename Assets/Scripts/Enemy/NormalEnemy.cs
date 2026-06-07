using UnityEngine;

/// <summary>
/// Enemy map bình thường. Set stats qua Inspector, không liên quan quest.
/// </summary>
public class NormalEnemy : BaseEnemy
{
    [Header("=== Stats ===")]
    [SerializeField] private int maxHealth = 60;
    [SerializeField] private float attackDamage = 8f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float defense = 0f;
    [SerializeField] private int exp = 10;
    [SerializeField] private int coin = 10;

    protected override int MaxHealth => maxHealth;
    protected override float AttackDamage => attackDamage;
    protected override float AttackCooldown => attackCooldown;
    protected override float Defense => defense;

    protected override void OnDeath()
    {
        PlayerStats.Instance.AddExp(exp);
        PlayerStats.Instance.AddCoin(coin);
        GetComponent<EnemyDropper>()?.Drop(transform.position);
        Debug.Log($"💀 NormalEnemy chết: {gameObject.name}");
        // TODO: drop item, spawn effect, v.v.
    }
    public void SlashSFX()
    {
        AudioManager.Instance.PlaySFX(2);
    }
}