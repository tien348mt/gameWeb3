using System.Collections;
using TMPro;
using UnityEngine;

public class LoadStatsPlayer : MonoBehaviour
{
    public static LoadStatsPlayer instance;
    public TextMeshProUGUI lv;
    public TextMeshProUGUI STR;
    public TextMeshProUGUI DEF;
    public TextMeshProUGUI Mana;

    private void Awake()
    {
        if(instance == null)
            instance = this;
        StartCoroutine(WaitForPlayerStats());
    }

    private void Start()
    {
        clear();
        lv.text += "Level: " + PlayerStats.Instance.level;
        STR.text += "STR: "+ PlayerStats.Instance.strength;
        DEF.text += "DEF: " + PlayerStats.Instance.defense;
        Mana.text += "Mana: " + PlayerStats.Instance.maxMana;
    }
    IEnumerator WaitForPlayerStats()
    {
        while (PlayerStats.Instance == null || PlayerStats.Instance.maxHp <= 0)
        {
            yield return null;
        }
    }
        void clear()
    {
        lv.text = "";
        STR.text = "";
        DEF.text = "";
        Mana.text = "";
    }
    public void Information()
    {
        clear();
        lv.text += "Level: " + PlayerStats.Instance.level;
        STR.text += "STR: " + PlayerStats.Instance.strength;
        DEF.text += "DEF: " + PlayerStats.Instance.defense;
        Mana.text += "Mana: " + PlayerStats.Instance.maxMana;
    }
}
