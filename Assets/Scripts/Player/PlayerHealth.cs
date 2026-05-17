using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth instance;
    public TextMeshProUGUI hp;
    public TextMeshProUGUI mana;
    public TextMeshProUGUI lv;
    public TextMeshProUGUI exp;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        StartCoroutine(WaitForPlayerStats());
    }

    IEnumerator WaitForPlayerStats()
    {
        while (PlayerStats.Instance == null || PlayerStats.Instance.maxHp <= 0)
            yield return null;

        UpdateUI();
        StartCoroutine(PassiveHeal());
    }

    // Hồi máu mỗi 2s, lượng hồi = currentHp / 100
    IEnumerator PassiveHeal()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);

            // healAmount là int (không còn float)
            int healAmount = Mathf.Max(Mathf.FloorToInt(PlayerStats.Instance.currentHp / 100f),4);

            PlayerStats.Instance.currentHp = Mathf.Min(
                PlayerStats.Instance.currentHp + healAmount,
                PlayerStats.Instance.maxHp
            );

            UpdateUI();
        }
    }

    public void TakeDame(int dmg)
    {
        float currentDMG = dmg - PlayerStats.Instance.defense;
        if (currentDMG <= 0) currentDMG = 10;
        PlayerStats.Instance.currentHp -= currentDMG;
        UpdateUI();
    }

    public void UplevelInformationUI() => UpdateUI();

    private void UpdateUI()
    {
        hp.text = "HP: " + PlayerStats.Instance.currentHp + "/" + PlayerStats.Instance.maxHp;
        mana.text = "Mana: " + PlayerStats.Instance.currentMana + "/" + PlayerStats.Instance.maxMana;
        lv.text = "Lv:" + PlayerStats.Instance.level;
        exp.text = "EXP: " + PlayerStats.Instance.currentExp + "/" + PlayerStats.Instance.requiredExp;
    }
}