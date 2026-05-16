using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Objective/Talk")]
public class TalkObjective : QuestObjective
{
    public string npcID;

    [Header("Gắn hội thoại")]
    public DialogueSO[] dialogueToPlay;

    public override bool CheckCompletion() => isCompleted;
}