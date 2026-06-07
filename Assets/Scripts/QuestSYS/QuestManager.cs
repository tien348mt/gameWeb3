using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public List<ActiveQuest> activeQuests = new List<ActiveQuest>();
    public List<string> completedQuests = new List<string>();
    public List<string> permanentlyDestroyedObjects = new List<string>();
    public bool IsLoaded { get; private set; } = false;

    [Header("=== Quest Database ===")]
    public List<QuestSO> allQuests; // kéo tất cả QuestSO vào đây trong Inspector

    private Dictionary<string, QuestSO> questSOCache = new Dictionary<string, QuestSO>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        // Build cache từ list kéo tay
        foreach (var q in allQuests)
            if (q != null && !questSOCache.ContainsKey(q.questID))
                questSOCache[q.questID] = q;
    }

    // ====================== LOAD TỪ FIREBASE ======================
    public void LoadAllProgress(List<QuestProgress> activeData, List<string> completedData, List<string> destroyedData)
    {
        completedQuests = completedData ?? new List<string>();
        permanentlyDestroyedObjects = destroyedData ?? new List<string>();

        activeQuests.Clear();
        foreach (var data in activeData)
        {
            // ── FIX BUG 1: restore đầy đủ objectives từ QuestSO ──
            if (questSOCache.TryGetValue(data.questID, out QuestSO so))
            {
                var quest = new ActiveQuest(so);
                // Đánh dấu lại objectives đã hoàn thành
                foreach (var obj in quest.objectives)
                    if (data.completedObjectives.Contains(obj.description))
                        obj.isCompleted = true;
                activeQuests.Add(quest);
            }
            else
            {
                Debug.LogWarning($"[QuestManager] Không tìm thấy QuestSO cho questID: {data.questID}");
            }
        }

        IsLoaded = true;

        foreach (var a in FindObjectsOfType<QuestObjectActivator>(true))
            a.CheckAndActivate();
    }

    public void AcceptQuest(QuestSO quest)
    {
        if (completedQuests.Contains(quest.questID) && !quest.isRepeatable) return;
        if (activeQuests.Exists(q => q.questID == quest.questID)) return; // tránh nhận trùng

        var newQuest = new ActiveQuest(quest);

        // ── FIX BUG 2: nếu đã kill enemy trước khi nhận quest, tính luôn ──
        foreach (var obj in newQuest.objectives)
        {
            if (obj is KillObjective k)
            {
                foreach (var destroyedID in permanentlyDestroyedObjects)
                    if (k.targetEnemyInstanceID == destroyedID)
                    {
                        k.currentAmount++;
                        if (k.CheckCompletion()) obj.isCompleted = true;
                    }
            }
        }

        activeQuests.Add(newQuest);
        SaveToFirebase();
    }

    // ====================== HOÀN THÀNH OBJECTIVE ======================
    public void CompleteObjective(string questID, string objectiveDesc)
    {
        var quest = activeQuests.Find(q => q.questID == questID);
        if (quest == null) return;

        var obj = quest.objectives.Find(o => o.description == objectiveDesc);
        if (obj != null) obj.isCompleted = true;

        if (quest.IsCompleted())
        {
            completedQuests.Add(quest.questID);
            activeQuests.Remove(quest);
            GiveReward(questID);
            SaveToFirebase();
            Debug.Log($"🏆 Quest {questID} HOÀN THÀNH TOÀN BỘ → Nhận thưởng!");
            FindObjectOfType<QuestUI>()?.OnQuestCompleted();
            foreach (var a in FindObjectsOfType<QuestObjectActivator>(true))
                a.CheckAndActivate();
        }
        else
        {
            SaveToFirebase();
            Debug.Log($"📌 Quest {questID} còn objective chưa hoàn thành.");
            FindObjectOfType<QuestUI>()?.OnObjectiveCompleted();
        }
    }

    // ====================== NHẬN THƯỞNG ======================
    private void GiveReward(string questID)
    {
        Debug.Log($"[GiveReward] Cache có {questSOCache.Count} quest: {string.Join(", ", questSOCache.Keys)}");

        QuestSO questSO = questSOCache.TryGetValue(questID, out var so) ? so : null;

        if (questSO == null)
        {
            Debug.LogWarning($"[GiveReward] ❌ Không tìm thấy QuestSO cho '{questID}' trong cache");
            return;
        }
        if (questSO.reward == null)
        {
            Debug.LogWarning($"[GiveReward] ❌ QuestSO '{questID}' không có reward");
            return;
        }

        Debug.Log($"🎁 Nhận thưởng: +{questSO.reward.gold} Gold, +{questSO.reward.exp} Exp");

        string wallet = ShowWalletAddress.Instance?.walletText?.text ?? "";
        if (!string.IsNullOrEmpty(wallet))
        {
            PlayerStats.Instance.AddExp(questSO.reward.exp);
            PlayerStats.Instance.AddCoin(questSO.reward.gold);
            RewardUI rewardUI = FindFirstObjectByType<RewardUI>();

            if (rewardUI != null)
            {
                rewardUI.coin.text = "COIN: " + questSO.reward.gold.ToString();
                rewardUI.exp.text = "EXP: " + questSO.reward.exp.ToString();
                AudioManager.Instance.PlaySFX(3);
                rewardUI.ShowReward();
            }
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
                    FindObjectOfType<QuestUI>()?.OnObjectiveCompleted();
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
                        FindObjectOfType<QuestUI>()?.OnObjectiveCompleted();
                        return;
                    }
                }
    }

    /*   public void NotifyEnemyKilled(string enemyInstanceID)
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
       }*/

    public void NotifyEnemyKilled(string enemyInstanceID)
    {
        if (permanentlyDestroyedObjects.Contains(enemyInstanceID)) return;
        permanentlyDestroyedObjects.Add(enemyInstanceID);

        foreach (var quest in activeQuests)
            foreach (var obj in quest.objectives)
                if (obj is KillObjective k && k.targetEnemyInstanceID == enemyInstanceID)
                {
                    k.currentAmount++;
                    if (k.CheckCompletion()) 
                    {
                        CompleteObjective(quest.questID, obj.description); // tự gọi UI nếu quest xong
                        
                    }
                    else
                    {
                        SaveToFirebase();
                        FindObjectOfType<QuestUI>()?.OnObjectiveCompleted();
                    }
                    return; // ← return sau khi đã xử lý UI
                }

        SaveToFirebase(); // enemy không thuộc quest nào
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
        var list = new List<QuestProgress>();
        foreach (var q in activeQuests)
            list.Add(new QuestProgress
            {
                questID = q.questID,
                completedObjectives = q.GetCompletedNames(),
                isCompleted = q.IsCompleted()
            });
        return list;
    }

    // ====================== INNER CLASSES ======================
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

        public bool IsCompleted() => objectives.TrueForAll(o => o.isCompleted);

        public List<string> GetCompletedNames()
        {
            var list = new List<string>();
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

    public void Logout()
    {
        if(PlayerStats.Instance != null)
        {
            PlayerStats.Instance.SaveData();
        }
    }
}