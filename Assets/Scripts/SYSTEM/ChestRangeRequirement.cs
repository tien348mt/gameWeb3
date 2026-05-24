using UnityEngine;

public class ChestRangeRequirement : MonoBehaviour
{
    public float interactRange = 2.5f;
    public string playerTag = "Player";
    public GameObject chestClosed;
    public GameObject chestOpen;

    [Header("Phần thưởng")]
    public int coinReward = 100;

    private Transform playerTransform;
    private bool isOpen = false;

    private void Start()
    {
        GameObject player = GameObject.FindWithTag(playerTag);
        if (player != null)
            playerTransform = player.transform;

        if (chestOpen != null)
            chestOpen.SetActive(false);
    }

    private void Update()
    {
        if (isOpen || playerTransform == null) return;

        bool inRange = Vector3.Distance(transform.position, playerTransform.position) <= interactRange;

        if (inRange && Input.GetKeyDown(KeyCode.F))
            OpenChest();
    }

    private void OpenChest()
    {
        isOpen = true;

        if (chestClosed != null)
            chestClosed.SetActive(false);

        if (chestOpen != null)
            chestOpen.SetActive(true);

        GiveCoinReward();
    }

    private void GiveCoinReward()
    {
        PlayerStats.Instance.AddCoin(coinReward);
        Debug.Log($"Nhận được {coinReward} coin!");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.25f);
        Gizmos.DrawSphere(transform.position, interactRange);
        Gizmos.color = new Color(0f, 1f, 0.5f, 1f);
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}