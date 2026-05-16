using System.Linq;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public CapsuleCollider2D col;
    public Rigidbody2D rb;
    public NPC_Patrol patrol;
    public GameObject interaction;
    public Animator animator;
    public string npcID = "npc1";

    private Vector3 defaultPosition;
    private NPC_QuestGiver questGiver;

    void Start()
    {
        defaultPosition = transform.position;
        questGiver = GetComponent<NPC_QuestGiver>();

        if (patrol != null) patrol.enabled = true;
        if (interaction != null) interaction.SetActive(false);

        if (rb != null)
            rb.bodyType = RigidbodyType2D.Kinematic;

        Debug.Log($"[NPC] {npcID} START - Position: {transform.position}");

        LoadSavedPosition();
    }

    private void LoadSavedPosition()
    {
        string wallet = ShowWalletAddress.Instance?.walletText?.text ?? "";
        if (string.IsNullOrEmpty(wallet))
        {
            Debug.LogWarning("[NPC] Wallet rỗng → không load position");
            return;
        }

        FirestoreManager.Instance.LoadNPCPosition(wallet, npcID, OnPositionLoaded);
    }

    private void OnPositionLoaded(Vector3 savedPos)
    {
        if (savedPos != Vector3.zero)
        {
            transform.position = savedPos;
            Debug.Log($"[NPC] Load vị trí từ Firebase: {savedPos}");
        }
        else
        {
            transform.position = defaultPosition;
            Debug.Log($"[NPC] Dùng default position");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log($"🟡 Player vào vùng NPC {npcID}");

        bool hasTalk = HasPendingTalkObjective();

        if (hasTalk)
        {
            if (patrol != null) patrol.enabled = false;

            if (interaction != null)
                interaction.SetActive(true);

            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;

            Debug.Log($"🟢 Có TalkObjective → bật interaction");
        }
        else
        {
            Debug.Log($"🔴 KHÔNG có TalkObjective → không bật interaction");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (patrol != null) patrol.enabled = true;

        if (interaction != null)
            interaction.SetActive(false);

        Debug.Log($"[NPC] Player rời NPC {npcID}");
    }

    // ====================== FIX LOGIC + DEBUG ======================
    private bool HasPendingTalkObjective()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogError("❌ QuestManager NULL");
            return false;
        }

        string questID = (questGiver != null && questGiver.questsToGive.Length > 0)
            ? questGiver.questsToGive[0].questID : "";

        Debug.Log($"[NPC] Check quest: {questID}");

        // ====================== CHƯA NHẬN QUEST ======================
        if (!IsQuestActive(questID))
        {
            Debug.Log("[NPC] Quest chưa active → check Element 0");

            var questSO = questGiver.questsToGive[0];

            if (questSO.objectives.Count > 0)
            {
                var first = questSO.objectives[0];

                Debug.Log($"[NPC] Element 0: {first.description} - {first.GetType().Name}");

                if (first is TalkObjective t)
                {
                    Debug.Log($"[NPC] Talk npcID={t.npcID} | NPC hiện tại={npcID}");
                    return t.npcID == npcID;
                }
            }

            return false;
        }

        // ====================== QUEST ĐANG CHẠY ======================
        var activeQuest = QuestManager.Instance.activeQuests.Find(q => q.questID == questID);

        if (activeQuest == null)
        {
            Debug.LogError("❌ activeQuest NULL");
            return false;
        }

        Debug.Log($"[NPC] Quest active có {activeQuest.objectives.Count} objectives");

        for (int i = 0; i < activeQuest.objectives.Count; i++)
        {
            var obj = activeQuest.objectives[i];

            Debug.Log($"   [{i}] {obj.description} | {obj.GetType().Name} | Completed={obj.isCompleted}");

            if (!obj.isCompleted)
            {
                Debug.Log($"👉 Objective hiện tại là index {i}");

                if (obj is TalkObjective t)
                {
                    Debug.Log($"👉 Đây là TalkObjective | npcID={t.npcID} | NPC={npcID}");

                    bool match = t.npcID == npcID;

                    if (!match)
                        Debug.LogWarning("❌ npcID không khớp");

                    return match;
                }
                else
                {
                    Debug.Log("❌ Objective hiện tại KHÔNG phải Talk → chưa đến lượt nói");
                    return false;
                }
            }
        }

        Debug.Log("❌ Không tìm thấy objective chưa hoàn thành");
        return false;
    }

    private bool IsQuestActive(string questID)
    {
        return QuestManager.Instance.activeQuests.Any(q => q.questID == questID);
    }

    public void BeginEscort()
    {
        Debug.Log($"🚀 BeginEscort NPC {npcID}");

        if (patrol != null) patrol.enabled = false;
        if (interaction != null) interaction.SetActive(false);

        rb.bodyType = RigidbodyType2D.Kinematic;

        if (animator != null)
            animator.Play("walk");
    }

    public void EndEscort()
    {
        Debug.Log($"🏁 EndEscort NPC {npcID}");

        rb.bodyType = RigidbodyType2D.Kinematic;

        if (interaction != null)
            interaction.SetActive(false);

        if (animator != null)
            animator.Play("idle");

        if (patrol != null)
            Invoke(nameof(EnablePatrol), 0.5f);
    }

    private void EnablePatrol()
    {
        if (patrol != null)
            patrol.enabled = true;
    }
}