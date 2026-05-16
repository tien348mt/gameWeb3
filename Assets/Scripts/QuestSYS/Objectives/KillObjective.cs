using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Objective/Kill")]
public class KillObjective : QuestObjective
{
    public string targetEnemyInstanceID;
    public int requiredAmount;
    [HideInInspector] public int currentAmount = 0;
    public override bool CheckCompletion() => currentAmount >= requiredAmount;
}