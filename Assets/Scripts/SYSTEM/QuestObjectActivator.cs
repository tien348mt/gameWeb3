using UnityEngine;

public class QuestObjectActivator : MonoBehaviour
{
    [Header("Quest")]
    public string questID;

    public void CheckAndActivate()
    {
        if (QuestManager.Instance.completedQuests.Contains(questID))
        {
            gameObject.SetActive(true);
            Debug.Log($"✅ Hiện {gameObject.name}");
        }
        else
        {
            Debug.Log($"❌ {gameObject.name}: quest '{questID}' chưa hoàn thành");
        }
    }
}