public interface IDamageable
{
    void TakeDamage(float damage);
}

public interface IKillable
{
    string GetKillID();
    void Die();
}