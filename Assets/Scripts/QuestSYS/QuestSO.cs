using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Quest Data")]
public class QuestSO : ScriptableObject
{
    public string questID;
    public string title;
    [TextArea(4, 10)] public string description;

    public List<QuestObjective> objectives = new List<QuestObjective>();
    public Reward reward;

    public QuestSO prerequisiteQuest;
    public bool isRepeatable = false;

    [Header("=== Dialogue sau khi hoàn thành quest ===")]
    public DialogueSO dialogueAfterCompleted;
}

[Serializable]
public class Reward
{
    public int gold;
    public int exp;
}

[Serializable]
public abstract class QuestObjective : ScriptableObject
{
    public string description;
    public bool isCompleted;
    public abstract bool CheckCompletion();
}