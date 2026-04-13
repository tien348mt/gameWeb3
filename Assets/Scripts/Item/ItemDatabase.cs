using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public List<ItemData> allItems;
    public ItemData GetItemById(string id)
    {
        ItemData foundItem = allItems.Find(item => item.itemId == id);

        if (foundItem != null)
        {
            Debug.Log($"<color=green>[Database]</color> Đã tìm thấy Item khớp với ID: {id}");
        }
        else
        {
            Debug.LogError($"<color=red>[Database] LỖI:</color> Không tìm thấy Item nào có ID là '{id}' trong AllItems!");
        }

        return foundItem;
    }

    public ItemData GetRandomArmorData()
    {
        if (allItems != null && allItems.Count > 0)
        {
            return allItems[Random.Range(0, allItems.Count)];
        }
        return null;
    }
}