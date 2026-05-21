using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject prefab;
    public Transform spawnPoint;
    public float range = 3f;
    public Animator animator;

    [Header("References")]
    public Transform player;

    private GameObject spawnedObject;
    private bool wasSpawned = false; // track đã spawn chưa

    void Awake()
    {
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }
    }

    void Update()
    {
        // Check nếu object vừa biến mất thì play Revert
        if (spawnedObject == null && wasSpawned)
        {
            animator?.Play("Revert");
            wasSpawned = false;
        }

        if (!Input.GetKeyDown(KeyCode.F)) return;
        if (player == null || prefab == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        if (dist > range)
        {
            Debug.Log($"[ObjectSpawner] Quá xa ({dist:F1}/{range})");
            return;
        }

        // Nếu object cũ vẫn còn thì không spawn thêm
        if (spawnedObject != null)
        {
            Debug.Log("[ObjectSpawner] Object chưa mất, không spawn thêm");
            return;
        }
        if (PlayerStats.Instance.currentMana < 40)
        {
            Debug.Log("Không đủ mana!");
            return;
        }
        // Spawn
        animator?.Play("Spawn");
        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;

        PlayerStats.Instance.currentMana -= 40;
        PlayerStats.Instance.SaveData();
        PlayerHealth.instance.UpdateUI();
        spawnedObject = Instantiate(prefab, pos, Quaternion.identity);
        wasSpawned = true;
        Debug.Log($"[ObjectSpawner] Spawn {prefab.name} tại {pos}");
    }

    // Gọi hàm này để check từ bên ngoài nếu cần
    public bool IsSpawnedAlive() => spawnedObject != null;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}