using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public List<ActiveQuest> activeQuests = new List<ActiveQuest>();
    public List<string> completedQuests = new List<string>();
    public List<string> permanentlyDestroyedObjects = new List<string>();
    public bool IsLoaded { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
    // ====================== LOAD TỪ FIREBASE ======================
    public void LoadAllProgress(List<QuestProgress> activeData, List<string> completedData, List<string> destroyedData)
    {
        completedQuests = completedData ?? new List<string>();
        permanentlyDestroyedObjects = destroyedData ?? new List<string>();

        activeQuests.Clear();
        foreach (var data in activeData)
            activeQuests.Add(new ActiveQuest(data));
        IsLoaded = true;

        foreach (var a in FindObjectsOfType<QuestObjectActivator>(true))
            a.CheckAndActivate();
    }

    public void AcceptQuest(QuestSO quest)
    {
        if (completedQuests.Contains(quest.questID) && !quest.isRepeatable) return;
        activeQuests.Add(new ActiveQuest(quest));
        SaveToFirebase();
    }

    // ====================== HOÀN THÀNH OBJECTIVE ======================
    public void CompleteObjective(string questID, string objectiveDesc)
    {
        var quest = activeQuests.Find(q => q.questID == questID);
        if (quest == null) return;

        // Đánh dấu objective vừa hoàn thành
        var obj = quest.objectives.Find(o => o.description == objectiveDesc);
        if (obj != null) obj.isCompleted = true;

        // KIỂM TRA XEM QUEST ĐÃ HOÀN THÀNH TẤT CẢ ELEMENT CHƯA
        if (quest.IsCompleted())
        {
            completedQuests.Add(quest.questID);
            activeQuests.Remove(quest);

            GiveReward(questID);      

            SaveToFirebase();
            Debug.Log($"🏆 Quest {questID} HOÀN THÀNH TOÀN BỘ → Nhận thưởng!"); 

            foreach (var a in FindObjectsOfType<QuestObjectActivator>(true))
                a.CheckAndActivate();

        }
        else
        {
            SaveToFirebase();
            Debug.Log($"📌 Quest {questID} còn objective chưa hoàn thành, chưa nhận thưởng.");
        }
    }

    // ====================== NHẬN THƯỞNG ======================
    private void GiveReward(string questID)
    {
        QuestSO questSO = null;

        // Tìm trong TẤT CẢ NPC_QuestGiver (fix cho nhiều NPC)
        NPC_QuestGiver[] allGivers = FindObjectsOfType<NPC_QuestGiver>();

        foreach (var giver in allGivers)
        {
            foreach (var q in giver.questsToGive)
            {
                if (q != null && q.questID == questID)
                {
                    questSO = q;
                    Debug.Log($"✅ Tìm thấy QuestSO cho quest {questID} từ NPC {giver.npcID}");
                    break;
                }
            }
            if (questSO != null) break;
        }

        if (questSO == null || questSO.reward == null)
        {
            Debug.LogWarning($"⚠️ Không tìm thấy QuestSO hoặc reward cho quest {questID}");
            return;
        }
        int gold = questSO.reward.gold;
        int exp = questSO.reward.exp;

        Debug.Log($"🎁 Nhận thưởng: +{gold} Gold, +{exp} Exp");

        string wallet = ShowWalletAddress.Instance?.walletText?.text ?? "";
        if (!string.IsNullOrEmpty(wallet))
        {
            PlayerStats.Instance.AddExp(exp);

            // PlayerStats.Instance.AddGold(gold);
        }
    }

    // ====================== NOTIFY ======================
    public void NotifyTalk(string npcID)
    {
        foreach (var quest in activeQuests)
            foreach (var obj in quest.objectives)
                if (obj is TalkObjective t && t.npcID == npcID)
                {
                    obj.isCompleted = true;
                    CompleteObjective(quest.questID, obj.description);
                    return;
                }
    }

    public void NotifyLocationReached(Vector3 playerPos, string triggerTag = "")
    {
        foreach (var quest in activeQuests)
            foreach (var obj in quest.objectives)
                if (obj is ReachLocationObjective reach)
                {
                    if (!string.IsNullOrEmpty(reach.triggerTag) && reach.triggerTag == triggerTag)
                        reach.reached = true;
                    else if (Vector3.Distance(playerPos, reach.targetPosition) <= reach.radius)
                        reach.reached = true;

                    if (reach.reached)
                    {
                        CompleteObjective(quest.questID, obj.description);
                        return;
                    }
                }
    }

    public void NotifyEnemyKilled(string enemyInstanceID)
    {
        if (permanentlyDestroyedObjects.Contains(enemyInstanceID)) return;
        permanentlyDestroyedObjects.Add(enemyInstanceID);

        foreach (var quest in activeQuests)
            foreach (var obj in quest.objectives)
                if (obj is KillObjective k && k.targetEnemyInstanceID == enemyInstanceID)
                {
                    k.currentAmount++;
                    if (k.CheckCompletion()) CompleteObjective(quest.questID, obj.description);
                    return;
                }
        SaveToFirebase();
    }

    public void NotifyEscortComplete(string escortNPCID, string destinationID)
    {
        foreach (var quest in activeQuests)
            foreach (var obj in quest.objectives)
                if (obj is EscortObjective e && e.escortNPCID == escortNPCID && e.destinationLocationID == destinationID)
                {
                    e.escortCompleted = true;
                    CompleteObjective(quest.questID, obj.description);
                    return;
                }
    }

    public bool IsDestroyed(string instanceID) => permanentlyDestroyedObjects.Contains(instanceID);

    // ====================== SAVE / LOAD ======================
    public void SaveToFirebase()
    {
        string wallet = ShowWalletAddress.Instance?.walletText?.text ?? "";
        if (string.IsNullOrEmpty(wallet)) return;

        FirestoreManager.Instance.SavePlayerQuests(wallet, GetActiveForSave(), completedQuests, permanentlyDestroyedObjects);
    }

    private List<QuestProgress> GetActiveForSave()
    {
        List<QuestProgress> list = new List<QuestProgress>();
        foreach (var q in activeQuests)
        {
            list.Add(new QuestProgress
            {
                questID = q.questID,
                completedObjectives = q.GetCompletedNames(),
                isCompleted = q.IsCompleted()
            });
        }
        return list;
    }

    [System.Serializable]
    public class ActiveQuest
    {
        public string questID;
        public string title;
        public List<QuestObjective> objectives = new List<QuestObjective>();

        public ActiveQuest(QuestSO data)
        {
            questID = data.questID;
            title = data.title;
            foreach (var o in data.objectives)
                objectives.Add(Instantiate(o));
        }

        public ActiveQuest(QuestProgress data)
        {
            questID = data.questID;
        }

        public bool IsCompleted() => objectives.TrueForAll(o => o.isCompleted);

        public List<string> GetCompletedNames()
        {
            List<string> list = new List<string>();
            foreach (var o in objectives)
                if (o.isCompleted) list.Add(o.description);
            return list;
        }
    }

    [System.Serializable]
    public class QuestProgress
    {
        public string questID;
        public List<string> completedObjectives = new List<string>();
        public bool isCompleted;
    }
}