using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemId;       // ID duy nhất (ví dụ: sword_01)
    public string itemName;     // Tên hiển thị
    public Sprite itemIcon;     // Ảnh trong túi đồ
    public string metadataUri;  // Link ảnh trên Pinata (nếu có sẵn)
    public int basePrice;       // Giá cơ bản
}