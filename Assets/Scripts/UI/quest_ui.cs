using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    [Header("Left Page")]
    public Transform leftContent;
    public GameObject questButtonPrefab;

    [Header("Right Page")]
    public TextMeshProUGUI questTitle;
    public TextMeshProUGUI rewardText;
    public Transform objectiveContent;
    public GameObject objectivePrefab;

    private List<GameObject> spawnedButtons = new List<GameObject>();
    private List<GameObject> spawnedObjectives = new List<GameObject>();
    private QuestManager.ActiveQuest currentDisplayedQuest;

    [Header("Toggle")]
    public CanvasGroup canvasGroup;
    private bool isOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
            ToggleUI();
    }

    void ToggleUI()
    {
        isOpen = !isOpen;
        canvasGroup.alpha = isOpen ? 1 : 0;
        canvasGroup.interactable = isOpen;
        canvasGroup.blocksRaycasts = isOpen;

        if (isOpen) RefreshQuestList();
    }

    public void RefreshQuestList()
    {
        foreach (var b in spawnedButtons) Destroy(b);
        spawnedButtons.Clear();

        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            var btn = Instantiate(questButtonPrefab, leftContent);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = quest.title;

            var q = quest;
            btn.GetComponent<Button>().onClick.AddListener(() => ShowQuestDetail(q));
            spawnedButtons.Add(btn);
        }

        if (QuestManager.Instance.activeQuests.Count == 0)
            ClearRightPage();
    }

    void ShowQuestDetail(QuestManager.ActiveQuest quest)
    {
        // Luôn lấy reference mới nhất từ activeQuests
        var fresh = QuestManager.Instance.activeQuests.Find(q => q.questID == quest.questID);
        if (fresh != null) quest = fresh;

        currentDisplayedQuest = quest;

        QuestSO so = null;
        foreach (var q in QuestManager.Instance.allQuests)
            if (q.questID == quest.questID) { so = q; break; }

        questTitle.text = quest.title;

        if (so != null && so.reward != null)
            rewardText.text = $":{so.reward.gold} Coin|EXP:{so.reward.exp}";
        else
            rewardText.text = "";

        foreach (var o in spawnedObjectives) Destroy(o);
        spawnedObjectives.Clear();

        foreach (var obj in quest.objectives)
        {
            var item = Instantiate(objectivePrefab, objectiveContent);
            string status = obj.isCompleted ? "[x]" : "[]";
            item.GetComponent<TextMeshProUGUI>().text = $"{status} {obj.description}";
            spawnedObjectives.Add(item);
        }
    }

    public void OnObjectiveCompleted()
    {
        // Nếu UI đang mở và đang hiển thị quest → refresh ngay
        if (isOpen && spawnedObjectives.Count > 0)
            RefreshCurrentObjectives();
        // Nếu UI đóng → khi mở lại sẽ tự RefreshQuestList → ShowQuestDetail với data mới
    }

    void RefreshCurrentObjectives()
    {
        if (currentDisplayedQuest == null) return;

        var quest = QuestManager.Instance.activeQuests
            .Find(q => q.questID == currentDisplayedQuest.questID);

        if (quest == null) return;

        for (int i = 0; i < spawnedObjectives.Count && i < quest.objectives.Count; i++)
        {
            string status = quest.objectives[i].isCompleted ? "[x]" : "[]";
            spawnedObjectives[i].GetComponent<TextMeshProUGUI>().text =
                $"{status} {quest.objectives[i].description}";
        }
    }

    public void OnQuestCompleted()
    {
        RefreshQuestList();
        ClearRightPage();
    }

    public void ClearRightPage()
    {
        questTitle.text = "";
        rewardText.text = "";
        foreach (var o in spawnedObjectives) Destroy(o);
        spawnedObjectives.Clear();
    }
}