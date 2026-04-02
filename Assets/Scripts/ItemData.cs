using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemId;       
    public string itemName;     
    public Sprite itemIcon;     
    public string metadataUri;  
    public string basePrice;     
    public string armor;
    public string attack;
}