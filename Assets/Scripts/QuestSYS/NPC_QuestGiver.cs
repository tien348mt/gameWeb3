using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

public class NPC_QuestGiver : MonoBehaviour
{
    [Header("=== NPC Info ===")]
    public string npcID = "npc1";

    [Header("=== Quest có thể giao ===")]
    public QuestSO[] questsToGive;

    [Header("=== UI ===")]
    public GameObject btnAccept;

    private bool isWaitingForAccept = false;
    private TalkObjective currentWaitingTalkObjective = null;
    private QuestSO currentSelectedQuest = null;

    private NPC_MoveAfterDialogue moveAfterDialogue;

    private void Awake()
    {
        if (btnAccept != null)
        {
            btnAccept.SetActive(false);

            Button button = btnAccept.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnAcceptQuest);
            }
        }

        moveAfterDialogue = GetComponent<NPC_MoveAfterDialogue>();
    }

    public void OnPlayerTalk()
    {
        isWaitingForAccept = false;
        if (btnAccept != null) btnAccept.SetActive(false);
        if (QuestManager.Instance == null || questsToGive == null || questsToGive.Length == 0)
        {
            Debug.LogError("❌ QuestManager.Instance is NULL hoặc NPC chưa có quest!");
            return;
        }

        currentSelectedQuest = GetCurrentQuestForThisNPC();

        if (currentSelectedQuest == null)
        {
            QuestSO lastCompletedQuest = GetLastCompletedQuestWithDialogue();

            if (lastCompletedQuest != null && lastCompletedQuest.dialogueAfterCompleted != null)
            {
                DialogueManager.instance.StartDialogue(lastCompletedQuest.dialogueAfterCompleted);
                Debug.Log($"🎉 Tất cả quest đã xong → Chơi dialogueAfterCompleted của {lastCompletedQuest.questID}");
            }
            else
            {
                Debug.LogWarning($"❌ NPC {npcID} không còn quest nào có thể nhận hoặc đang chạy.");
            }

            return;
        }

        string questID = currentSelectedQuest.questID;
        Debug.Log($"[NPC_QuestGiver] NPC: {npcID} | Quest đang xử lý: {questID}");

        TalkObjective talkObj = FindNextTalkObjective(currentSelectedQuest);

        if (talkObj == null)
        {
            Debug.LogWarning($"❌ Không tìm thấy TalkObjective đang chờ cho NPC {npcID} trong quest {questID}");
            return;
        }

        currentWaitingTalkObjective = talkObj;

        if (talkObj.dialogueToPlay != null && talkObj.dialogueToPlay.Length > 0)
        {
            StartCoroutine(PlayDialogueSequence(talkObj.dialogueToPlay));
        }
        else
        {
            Debug.LogWarning($"⚠️ TalkObjective {talkObj.description} chưa có dialogueToPlay!");
        }
    }

    private QuestSO GetCurrentQuestForThisNPC()
    {
        // Ưu tiên quest đang active NHƯNG objective hiện tại phải thuộc NPC này
        foreach (QuestSO quest in questsToGive)
        {
            if (quest == null) continue;
            if (!IsQuestActive(quest.questID)) continue;

            var activeQuest = QuestManager.Instance.activeQuests
                .FirstOrDefault(q => q.questID == quest.questID);

            if (activeQuest == null) continue;

            QuestObjective currentObj = activeQuest.objectives
                .FirstOrDefault(o => !o.isCompleted);

            if (currentObj is TalkObjective talk && talk.npcID == npcID)
            {
                Debug.Log($"✅ NPC {npcID} chọn active quest {quest.questID} vì current objective là Talk của NPC này");
                return quest;
            }

            Debug.Log($"⛔ NPC {npcID} bỏ qua active quest {quest.questID} vì current objective là {currentObj?.GetType().Name}");
        }

        // Nếu chưa có quest active phù hợp, tìm quest mới có thể nhận
        foreach (QuestSO quest in questsToGive)
        {
            if (quest == null) continue;

            bool completed = QuestManager.Instance.completedQuests.Contains(quest.questID);

            if (completed && !quest.isRepeatable)
                continue;

            if (IsQuestActive(quest.questID))
                continue;

            if (quest.prerequisiteQuest != null &&
                !QuestManager.Instance.completedQuests.Contains(quest.prerequisiteQuest.questID))
            {
                Debug.Log($"⛔ Quest {quest.questID} chưa mở khóa. Cần hoàn thành {quest.prerequisiteQuest.questID}");
                continue;
            }

            if (quest.objectives.Count > 0 &&
                quest.objectives[0] is TalkObjective firstTalk &&
                firstTalk.npcID == npcID)
            {
                Debug.Log($"✅ NPC {npcID} chọn quest mới {quest.questID}");
                return quest;
            }
        }

        Debug.LogWarning($"❌ NPC {npcID} không có quest phù hợp tại thời điểm này");
        return null;
    }

    private QuestSO GetLastCompletedQuestWithDialogue()
    {
        for (int i = questsToGive.Length - 1; i >= 0; i--)
        {
            QuestSO quest = questsToGive[i];
            if (quest == null) continue;

            if (QuestManager.Instance.completedQuests.Contains(quest.questID) &&
                quest.dialogueAfterCompleted != null)
            {
                return quest;
            }
        }

        return null;
    }

    private TalkObjective FindNextTalkObjective(QuestSO quest)
    {
        string questID = quest.questID;

        if (!IsQuestActive(questID))
        {
            if (quest.objectives != null && quest.objectives.Count > 0)
            {
                var firstObj = quest.objectives[0];

                if (firstObj is TalkObjective t && t.npcID == npcID)
                    return t;

                Debug.LogWarning($"⚠️ Element 0 của quest {questID} không phải TalkObjective của NPC {npcID}");
            }

            return null;
        }

        var activeQuest = QuestManager.Instance.activeQuests.FirstOrDefault(q => q.questID == questID);
        if (activeQuest == null) return null;

        for (int i = 0; i < activeQuest.objectives.Count; i++)
        {
            var obj = activeQuest.objectives[i];

            if (!obj.isCompleted)
            {
                if (obj is TalkObjective t && t.npcID == npcID)
                    return t;

                Debug.Log($"📌 Objective hiện tại là {obj.GetType().Name} → Chưa đến lượt Talk");
                return null;
            }
        }

        return null;
    }

    private bool IsQuestActive(string questID)
    {
        return QuestManager.Instance.activeQuests.Any(q => q.questID == questID);
    }

    private IEnumerator PlayDialogueSequence(DialogueSO[] dialogues)
    {
        if (dialogues == null || dialogues.Length == 0) yield break;

        for (int i = 0; i < dialogues.Length - 1; i++)
        {
            bool finished = false;
            DialogueManager.instance.StartDialogue(dialogues[i], () => finished = true);
            yield return new WaitUntil(() => finished);
        }

        DialogueSO lastDialogue = dialogues[dialogues.Length - 1];
        bool lastLineReached = false;

        UnityAction listener = null;
        listener = () =>
        {
            lastLineReached = true;
            DialogueManager.instance.onLastLineReached.RemoveListener(listener);

            isWaitingForAccept = true;
            DialogueManager.instance.isWaitingForAccept = true;
            if (btnAccept != null)
            {
                Button button = btnAccept.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(OnAcceptQuest);
                }

                btnAccept.SetActive(true);
            }

            Debug.Log("🟢 Đến dòng cuối cùng → Hiện nút Accept!");
        };

        DialogueManager.instance.onLastLineReached.AddListener(listener);
        DialogueManager.instance.StartDialogue(lastDialogue, null);

        yield return new WaitUntil(() => lastLineReached);
    }

    public void OnAcceptQuest()
    {
        Debug.Log("🔥 OnAcceptQuest() được gọi!");
        DialogueManager.instance.isWaitingForAccept = false;
        if (currentSelectedQuest == null)
            currentSelectedQuest = GetCurrentQuestForThisNPC();

        if (currentSelectedQuest == null)
        {
            Debug.LogError("❌ Không tìm thấy quest hiện tại để Accept");
            return;
        }

        if (currentWaitingTalkObjective == null)
        {
            currentWaitingTalkObjective = FindCurrentTalkForThisNPC(currentSelectedQuest.questID);
        }

        if (currentWaitingTalkObjective == null)
        {
            Debug.LogError("❌ Không tìm thấy TalkObjective nào để Accept");
            return;
        }

        isWaitingForAccept = false;
        if (btnAccept != null) btnAccept.SetActive(false);

        if (DialogueManager.instance != null && DialogueManager.instance.isDialogueActive)
            DialogueManager.instance.EndDialogue();

        string questID = currentSelectedQuest.questID;
        string objectiveDesc = currentWaitingTalkObjective.description;

        if (!IsQuestActive(questID))
        {
            QuestManager.Instance.AcceptQuest(currentSelectedQuest);
            Debug.Log($"✅ Đã nhận quest: {questID}");
        }

        QuestManager.Instance.CompleteObjective(questID, objectiveDesc);

        currentWaitingTalkObjective = null;

        CheckNextObjectiveAction(questID);

        currentSelectedQuest = null;

        Debug.Log("🎉 OnAcceptQuest() hoàn tất!");
    }

    private TalkObjective FindCurrentTalkForThisNPC(string questID)
    {
        var activeQuest = QuestManager.Instance.activeQuests
            .FirstOrDefault(q => q.questID == questID);

        if (activeQuest == null)
        {
            Debug.LogError($"❌ Không tìm thấy activeQuest: {questID}");
            return null;
        }

        foreach (var obj in activeQuest.objectives)
        {
            if (!obj.isCompleted)
            {
                if (obj is TalkObjective t && t.npcID == npcID)
                {
                    Debug.Log($"✅ Đúng lượt Talk của NPC {npcID}: {t.description}");
                    return t;
                }

                Debug.LogWarning($"⛔ Objective hiện tại của quest {questID} không phải Talk của NPC {npcID}. Current = {obj.GetType().Name}");
                return null;
            }
        }

        return null;
    }

    private void CheckNextObjectiveAction(string questID)
    {
        var activeQuest = QuestManager.Instance.activeQuests.FirstOrDefault(q => q.questID == questID);

        if (activeQuest == null)
        {
            Debug.Log($"🏁 Quest {questID} đã hoàn thành hoặc không còn active.");
            return;
        }

        QuestObjective nextObjective = activeQuest.objectives.FirstOrDefault(o => !o.isCompleted);

        if (nextObjective == null)
            return;

        if (nextObjective is ReachLocationObjective)
        {
            if (moveAfterDialogue != null)
            {
                moveAfterDialogue.StartEscortImmediately();
                Debug.Log("🚀 Next objective là Reach → Bắt đầu di chuyển NPC");
            }
        }
        else if (nextObjective is KillObjective)
        {
            Debug.Log("⚔️ Next objective là Kill → Chờ player tiêu diệt quái");
        }
        else if (nextObjective is TalkObjective)
        {
            Debug.Log("💬 Next objective là Talk → Sẵn sàng nói chuyện tiếp");
        }
    }
}