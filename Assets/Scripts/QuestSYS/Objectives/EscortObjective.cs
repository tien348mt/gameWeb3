using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Objective/Escort")]
public class EscortObjective : QuestObjective
{
    public string escortNPCID;
    public string destinationLocationID;
    [HideInInspector] public bool escortCompleted = false;
    public override bool CheckCompletion() => escortCompleted;
}