using DG.Tweening;
using UnityEngine;

/// <summary>
/// Base class chứa toàn bộ AI, di chuyển, animation.
/// Subclass chỉ cần override stats và OnDeath().
/// </summary>
public abstract class BaseEnemy : MonoBehaviour, IDamageable, IKillable
{
    [Header("=== AI ===")]
    [SerializeField] private Transform player;
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float moveSpeed = 3f;

    [Header("=== Debug ===")]
    [SerializeField] private bool showAttackGizmos = true;
    // ── Stats do subclass cung cấp ───────────────────────────
    protected abstract int MaxHealth { get; }
    protected abstract float AttackDamage { get; }
    protected abstract float AttackCooldown { get; }
    protected abstract float Defense { get; }

    // ── Animator state names ─────────────────────────────────
    private const string StateIdle = "idle";
    private const string StateWalk = "Walk";
    private const string StateAttack = "Attack";
    private const string StateDie = "Die";

    // ── Internal ─────────────────────────────────────────────
    private Animator anim;
    private Vector3 startPosition;

    private float attackTimer = 0f;
    private int currentHealth;

    private bool isDead = false;
    private bool isAttacking = false;
    private Vector2 lastAttackPosition;


    [Header("=== HP Bar ===")]
    [SerializeField] private Transform hpBarFill;
    private float _hpBarFullScaleX;
    

    private enum AIState
    {
        Idle,
        Chase,
        Attack,
        Return
    }

    private AIState currentState = AIState.Idle;

    private AIState previousState = AIState.Idle;
    // ════════════════════════════════════════════════════════
    //  Unity lifecycle
    // ════════════════════════════════════════════════════════

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();

        startPosition = transform.position;

        currentHealth = MaxHealth;

        if (player == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");

            if (go != null)
                player = go.transform;
        }
        if (hpBarFill != null)
            _hpBarFullScaleX = hpBarFill.localScale.x;
    }

    void Update()
    {
        if (isDead || player == null)
            return;

        // Nếu đang attack animation thì không xử lý state khác
        if (isAttacking)
            return;

        // Cooldown attack
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, player.position);

        // ── Xác định state ─────────────────────────────────

        if (dist <= attackRange)
        {
            // Trong range đánh

            if (attackTimer <= 0f)
                currentState = AIState.Attack;
            else
                currentState = AIState.Chase;
        }
        else if (dist <= detectionRange)
        {
            // Chase player
            currentState = AIState.Chase;
        }
        else if (currentState != AIState.Idle)
        {
            // Return về vị trí cũ
            currentState = AIState.Return;
        }

        // ── Execute state ─────────────────────────────────

        switch (currentState)
        {
            case AIState.Idle:
                DoIdle();
                break;

            case AIState.Chase:
                DoChase();
                break;

            case AIState.Attack:
                DoAttack();
                break;

            case AIState.Return:
                DoReturn();
                break;
        }

        if (currentState != previousState)
        {
            OnStateChanged(currentState);
            previousState = currentState;
        }
    }

    // ════════════════════════════════════════════════════════
    //  AI States
    // ════════════════════════════════════════════════════════

    void DoIdle()
    {
        PlayAnim(StateIdle);
    }

    void DoChase()
    {
        PlayAnim(StateWalk);

        MoveToward(player.position);

        FlipToward(player.position);
    }

    void DoAttack()
    {
        FlipToward(player.position);

        if (attackTimer <= 0f)
        {
            attackTimer = AttackCooldown;

            isAttacking = true;

            anim.Play(StateAttack);

            //DealDamage();
        }
    }

    void DoReturn()
    {
        if (Vector3.Distance(transform.position, startPosition) > 0.05f)
        {
            PlayAnim(StateWalk);

            MoveToward(startPosition);

            FlipToward(startPosition);
        }
        else
        {
            transform.position = startPosition;

            attackTimer = 0f;

            currentState = AIState.Idle;
        }
    }

    // ════════════════════════════════════════════════════════
    //  Animation Event
    // ════════════════════════════════════════════════════════

    /// <summary>
    /// Gọi ở frame cuối animation Attack bằng Animation Event
    /// </summary>
    public void EndAttack()
    {
        DealDamage();
        isAttacking = false;
    }
    public void AttackLT()
    {
        DealDamage();
    }

    // ════════════════════════════════════════════════════════
    //  IDamageable / IKillable
    // ════════════════════════════════════════════════════════

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        int finalDamage = Mathf.Max(
            1,
            Mathf.RoundToInt(damage * (1f - Defense / 100f))
        );

        currentHealth -= finalDamage;

        UpdateHPBar();

        Debug.Log(
            $"[{gameObject.name}] nhận {finalDamage} dame (def {Defense}%), còn {currentHealth}/{MaxHealth}"
        );

        if (currentHealth <= 0)
            Die();
    }

    private void UpdateHPBar()
    {
        if (hpBarFill == null) return;

        float ratio = Mathf.Clamp01((float)currentHealth / MaxHealth); // ← Clamp01 giới hạn 0~1
        DOTween.Kill(hpBarFill);
        hpBarFill.DOScaleX(_hpBarFullScaleX * ratio, 0.3f).SetEase(Ease.OutCubic);
    }

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;

        anim.Play(StateDie);

        OnDeath();

        Destroy(gameObject, 1.5f);
    }

    public virtual string GetKillID()
    {
        return gameObject.name;
    }

    // ════════════════════════════════════════════════════════
    //  Subclass hooks
    // ════════════════════════════════════════════════════════

    /// <summary>
    /// Override để thêm logic khi chết
    /// </summary>
    protected virtual void OnDeath()
    {

    }

    /// <summary>
    /// Override nếu cần custom damage/effect
    /// </summary>
    /*protected virtual void DealDamage()
    {
        if (player == null)
            return;

        // Hướng enemy đang nhìn
        float dir = transform.localScale.x > 0 ? 1f : -1f;

        // Vị trí hit phía trước mặt enemy
        Vector2 attackPos = (Vector2)transform.position + Vector2.right * dir * attackRange * 0.5f;

        // Save để vẽ gizmos
        lastAttackPosition = attackPos;

        Collider2D hit = Physics2D.OverlapCircle(
            attackPos,
            attackRange
        );

        if (hit != null && hit.CompareTag("Player"))
        {
            if (DamagePopupPool.Instance != null)
            {
                DamagePopupPool.Instance.ShowDamage(
                    (int)AttackDamage,
                    hit.transform.position
                );
            }
            PlayerHealth.instance?.TakeDame((int)AttackDamage);
            Debug.Log($"💥 Enemy hit player: {hit.name}");
        }
    }*/
    protected virtual void DealDamage()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // Kiểm tra khoảng cách thực tế (dùng attackRange đã set trong Inspector)
        if (dist <= attackRange + 0.3f)   // +0.3f để dễ trúng hơn một chút
        {
            float finalDamage = AttackDamage;

            if (DamagePopupPool.Instance != null)
            {
                DamagePopupPool.Instance.ShowDamage((int)finalDamage, player.position);
            }

            PlayerHealth.instance?.TakeDame((int)finalDamage);

            Debug.Log($"💥 {gameObject.name} đánh trúng Player gây {finalDamage} damage");
        }
    }
    // ════════════════════════════════════════════════════════
    //  Helpers
    // ════════════════════════════════════════════════════════

    void MoveToward(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;

        dir.z = 0f;

        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    void FlipToward(Vector3 target)
    {
        float dirX = target.x - transform.position.x;

        if (Mathf.Abs(dirX) > 0.01f)
        {
            Vector3 s = transform.localScale;

            s.x = Mathf.Abs(s.x) * (dirX < 0 ? -1f : 1f);

            transform.localScale = s;
        }
    }

    void PlayAnim(string stateName)
    {
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            anim.Play(stateName);
        }
    }

    // ════════════════════════════════════════════════════════
    //  Gizmos
    // ════════════════════════════════════════════════════════

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Attack hitbox
        if (showAttackGizmos)
        {
            float dir = transform.localScale.x > 0 ? 1f : -1f;

            Vector2 attackPos = (Vector2)transform.position + Vector2.right * dir * attackRange * 0.5f;

            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(attackPos, attackRange);
        }
    }

    void OnStateChanged(AIState to)
    {
        if (to == AIState.Return || to == AIState.Idle)
            CombatMusicManager.Instance?.ExitCombat(this);
        else
            CombatMusicManager.Instance?.EnterCombat(this);
    }
    // Thêm vào OnDestroy để tránh enemy chết còn nằm trong list
    protected virtual void OnDestroy()
    {
        CombatMusicManager.Instance?.ExitCombat(this);
    }


}