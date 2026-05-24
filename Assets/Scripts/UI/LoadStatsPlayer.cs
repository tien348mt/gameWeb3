using TMPro;
using UnityEngine;
using System.Collections;

public class LoadStatsPlayer : MonoBehaviour
{
    public static LoadStatsPlayer instance;

    public TextMeshProUGUI lv;
    public TextMeshProUGUI STR;
    public TextMeshProUGUI DEF;
    public TextMeshProUGUI Mana;

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

        // Bắt đầu chờ PlayerStats load xong
        StartCoroutine(WaitForPlayerStats());
    }

    IEnumerator WaitForPlayerStats()
    {
        // Chờ PlayerStats được khởi tạo và load data từ Firebase
        while (PlayerStats.Instance == null || PlayerStats.Instance.maxHp <= 0)
        {
            yield return new WaitForSeconds(0.2f);
        }

        Debug.Log("✅ LoadStatsPlayer: PlayerStats đã load xong → Cập nhật UI");
        UpdateStats();
    }

    // Hàm công khai - PlayerStats sẽ gọi hàm này mỗi khi data thay đổi
    public void UpdateStats()
    {
        if (PlayerStats.Instance == null) return;

        lv.text = "Level: " + PlayerStats.Instance.level;
        STR.text = "STR: " + PlayerStats.Instance.strength;
        DEF.text = "DEF: " + PlayerStats.Instance.defense;
        Mana.text = "Mana: " + PlayerStats.Instance.maxMana;

        Debug.Log("📊 LoadStatsPlayer đã cập nhật UI thành công");
    }

    // Hàm cũ (để tương thích code cũ nếu bạn đang gọi ở đâu đó)
    public void Information()
    {
        UpdateStats();
    }

    // Hàm clear (nếu cần dùng)
    public void Clear()
    {
        lv.text = "";
        STR.text = "";
        DEF.text = "";
        Mana.text = "";
    }
}