using UnityEngine;

public class NPC_MoveAfterDialogue : MonoBehaviour
{
    [Header("=== Quest Info ===")]
    public string questID;

    [Header("=== Movement ===")]
    public float moveSpeed = 3.5f;
    public float arriveDistance = 0.6f;

    [Header("=== Interaction ===")]
    public GameObject interaction;
    public GameObject talkRange;

    private Animator animator;
    private NPC npcComponent;
    private Rigidbody2D rb;

    private bool isMoving = false;
    private Vector2 targetPos;
    private ReachLocationObjective currentReachObjective;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        npcComponent = GetComponent<NPC>();
        rb = GetComponent<Rigidbody2D>();

        if (interaction != null)
            interaction.SetActive(false);

        if (rb != null && rb.bodyType == RigidbodyType2D.Static)
        {
            Debug.LogWarning("⚠️ NPC đang dùng Rigidbody2D Static. Đổi sang Kinematic để di chuyển.");
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    public void StartEscortImmediately()
    {
        Debug.Log($"🚀 StartEscortImmediately được gọi! NPC: {npcComponent?.npcID}");

        if (QuestManager.Instance == null)
        {
            Debug.LogError("❌ QuestManager.Instance = null");
            return;
        }

        var activeQuest = QuestManager.Instance.activeQuests.Find(q => q.questID == questID);

        if (activeQuest == null)
        {
            Debug.LogError($"❌ Không tìm thấy active quest: {questID}");
            return;
        }

        currentReachObjective = null;

        for (int i = 0; i < activeQuest.objectives.Count; i++)
        {
            QuestObjective obj = activeQuest.objectives[i];

            if (!obj.isCompleted && obj is ReachLocationObjective reach)
            {
                currentReachObjective = reach;
                Debug.Log($"✅ Tìm thấy ReachLocationObjective [{i}]: {reach.description}");
                break;
            }
        }

        if (currentReachObjective == null)
        {
            Debug.LogWarning("⚠️ Không tìm thấy ReachLocationObjective chưa hoàn thành.");
            return;
        }

        targetPos = currentReachObjective.targetPosition;
        isMoving = true;

        if (npcComponent != null)
            npcComponent.BeginEscort();

        if (animator != null)
            animator.SetBool("isWalking", true);

        if (interaction != null)
            interaction.SetActive(false);

        if (talkRange != null)
            talkRange.SetActive(false);

        Debug.Log($"✅ NPC bắt đầu di chuyển đến: {targetPos}");
    }

    private void FixedUpdate()
    {
        if (!isMoving) return;

        Vector2 currentPos = rb != null ? rb.position : (Vector2)transform.position;
        Vector2 direction = (targetPos - currentPos).normalized;

        Vector2 newPos = Vector2.MoveTowards(
            currentPos,
            targetPos,
            moveSpeed * Time.fixedDeltaTime
        );

        if (rb != null && rb.bodyType != RigidbodyType2D.Static)
        {
            rb.MovePosition(newPos);
        }
        else
        {
            transform.position = newPos;
        }

        if (Mathf.Abs(direction.x) > 0.01f)
        {
            float scaleX = direction.x > 0 ? 1f : -1f;
            transform.localScale = new Vector3(
                scaleX,
                transform.localScale.y,
                transform.localScale.z
            );
        }

        if (Vector2.Distance(newPos, targetPos) <= arriveDistance)
        {
            ArrivedAtDestination();
        }
    }

    private void ArrivedAtDestination()
    {
        if (!isMoving) return;

        isMoving = false;

        if (npcComponent != null)
            npcComponent.EndEscort();

        if (animator != null)
            animator.SetBool("isWalking", false);

        SaveNPCPosition();

        if (currentReachObjective != null)
        {
            QuestManager.Instance.CompleteObjective(
                questID,
                currentReachObjective.description
            );

            Debug.Log($"✅ Hoàn thành ReachLocation: {currentReachObjective.description}");
        }

        CheckNextQuestStep();

        currentReachObjective = null;
    }

    private void SaveNPCPosition()
    {
        string wallet = ShowWalletAddress.Instance?.walletText?.text ?? "";

        if (string.IsNullOrEmpty(wallet))
            return;

        if (FirestoreManager.Instance == null)
        {
            Debug.LogWarning("⚠️ FirestoreManager.Instance = null, không lưu vị trí NPC.");
            return;
        }

        if (npcComponent == null)
        {
            Debug.LogWarning("⚠️ npcComponent = null, không lưu vị trí NPC.");
            return;
        }

        FirestoreManager.Instance.SaveNPCPosition(
            wallet,
            npcComponent.npcID,
            transform.position
        );

        Debug.Log($"💾 Đã lưu vị trí NPC {npcComponent.npcID}: {transform.position}");
    }

    private void CheckNextQuestStep()
    {
        var activeQuest = QuestManager.Instance.activeQuests.Find(q => q.questID == questID);
        if (activeQuest == null) return;

        QuestObjective next = activeQuest.objectives.Find(o => !o.isCompleted);

        if (next == null)
        {
            Debug.Log("🎉 Quest hoàn thành");
            return;
        }

        if (next is TalkObjective)
        {
            Debug.Log("💬 Next = TALK");

            if (talkRange != null)
                talkRange.SetActive(true);

            if (interaction != null)
                interaction.SetActive(true);

            Collider2D player = Physics2D.OverlapCircle(transform.position, 2f, LayerMask.GetMask("Player"));

            if (player != null)
            {
                Debug.Log("⚡ Player đang đứng sẵn → bật interaction ngay");
                if (interaction != null)
                    interaction.SetActive(true);
            }
        }
    }
}