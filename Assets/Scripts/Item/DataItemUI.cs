// DataItemUI.cs
using TMPro;
using UnityEngine;

public class DataItemUI : MonoBehaviour
{
    public TextMeshProUGUI hp;
    public TextMeshProUGUI mana;
    public TextMeshProUGUI armor;
    public TextMeshProUGUI dmg;
  
    public void SetValueData(ItemData item) // nhận ItemData trực tiếp
    {
        if (item == null) return;
        hp.text = "HP: " + item.hp;
        mana.text = "Mana: " + item.mana;
        armor.text = "Armor: " + item.armor;
        dmg.text = "DMG: " + item.attack;
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}