using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth instance;
    public TextMeshProUGUI hp;
    public TextMeshProUGUI mana;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        StartCoroutine(WaitForPlayerStats());
    }
    void Start()
    {
       
    }

    private void Update()
    {
       
    }
    IEnumerator WaitForPlayerStats()
    {
        while (PlayerStats.Instance == null || PlayerStats.Instance.maxHp <= 0)
        {
            yield return null;
        }
        hp.text = "HP: " + PlayerStats.Instance.currentHp + "/" + PlayerStats.Instance.maxHp;
        mana.text = "Mana: " + PlayerStats.Instance.currentMana + "/" + PlayerStats.Instance.maxMana;
    }
    public void UplevelInformationUI()
    {
        hp.text = "HP: " + PlayerStats.Instance.currentHp + "/" + PlayerStats.Instance.maxHp;
        mana.text = "Mana: " + PlayerStats.Instance.currentMana + "/" + PlayerStats.Instance.maxMana;
    }
    public void TakeDame(int dmg)
    {
        float currentDMG = dmg - PlayerStats.Instance.defense;
        if (currentDMG <= 0) currentDMG = 10;
        PlayerStats.Instance.currentHp -= currentDMG;
        hp.text = "HP: " + PlayerStats.Instance.currentHp + "/" + PlayerStats.Instance.maxHp;
        mana.text = "Mana: " + PlayerStats.Instance.currentMana + "/" + PlayerStats.Instance.maxMana;
    }
}
