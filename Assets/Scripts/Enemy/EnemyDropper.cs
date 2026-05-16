using System.Collections;
using UnityEngine;

/// <summary>
/// Gắn component này lên bất kỳ enemy nào để cho drop item khi chết.
/// Gọi Drop() từ OnDeath() của enemy.
/// </summary>
public class EnemyDropper : MonoBehaviour
{
    [Header("Drop Settings")]
    public ItemDatabase itemDatabase;
    public GameObject armorPrefab;

    [Range(0f, 100f)]
    public float dropChance = 50f;

    void Awake()
    {
        if (itemDatabase == null)
            itemDatabase = FindObjectOfType<ItemDatabase>();
    }

    public void Drop(Vector3 dropPosition)
    {
        StartCoroutine(WaitAndDrop(dropPosition));
    }

    IEnumerator WaitAndDrop(Vector3 dropPosition)
    {
        Animator anim = GetComponent<Animator>();

        if (anim != null)
        {
            // Đợi animation Die bắt đầu
            yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Die"));
            // Đợi animation Die chạy xong frame cuối
            yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
        }

        if (itemDatabase == null || armorPrefab == null) yield break;
        if (Random.Range(0f, 100f) > dropChance) yield break;

        ItemData randomData = itemDatabase.GetRandomArmorData();
        if (randomData == null) yield break;

        GameObject newItem = Instantiate(armorPrefab, dropPosition, Quaternion.identity);
        Collectible collectible = newItem.GetComponent<Collectible>();
        collectible?.Setup(randomData);
    }
}