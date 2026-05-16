using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject slash;
    public void OnAttacck()
    {
        slash.SetActive(true);
    }
    public void EndAttack()
    {
        slash.SetActive(false);
    }
}
