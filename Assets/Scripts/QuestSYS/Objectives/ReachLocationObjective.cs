using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Objective/Reach Location")]
public class ReachLocationObjective : QuestObjective
{
    public Vector3 targetPosition;
    public float radius = 4f;
    public string triggerTag = "";
    [HideInInspector] public bool reached = false;
    public override bool CheckCompletion() => reached;
}