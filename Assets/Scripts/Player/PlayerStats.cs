using UnityEngine;
using System.Threading.Tasks;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Player Data")]
    public string walletAddress;
    public int level = 1;
    public int currentExp = 0;

    [Header("Stats CSV")]
    public int requiredExp;
    public float maxHp;
    public float maxMana;
    public float strength;
    public float defense;

    [Header("Current Status")]
    public float currentHp;
    public float currentMana;
    public float currentSTR;
    public float currentDEF;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void Start()
    {
        var address = await ThirdwebManager.Instance.SDK.wallet.GetAddress();
        walletAddress = address;
        LoadPlayerData();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F)) 
        {
            AddExp(5);
            currentHp -= 5;
        }
    }
    public void LoadPlayerData()
    {
        FirestoreManager db = FindObjectOfType<FirestoreManager>();
        if (db != null)
        {
            db.LoadPlayerStats(walletAddress, (lv, exp, pos) =>
            {
                this.level = lv;
                this.currentExp = exp;
                StartCoroutine(WaitAndUpdateStats(lv, pos));
            });
        }
    }

    private IEnumerator WaitAndUpdateStats(int lv, Vector3 pos)
    { 
        while (StatManager.Instance == null) yield return null;
        UpdateStatsFromCSV(lv);
        if (pos != Vector3.zero) transform.position = pos;
        Debug.Log(">>> Đã nạp chỉ số từ CSV thành công!");
    }
    private void UpdateStatsFromCSV(int lv)
    {
        var data = StatManager.Instance.GetDataByLevel(lv);

        if (data != null)
        {
            this.level = data.Level;
            this.maxHp = data.HP;
            this.maxMana = data.MANA;
            this.strength = data.STR;
            this.defense = data.DEF;
            this.requiredExp = data.EXP;

            this.currentHp = this.maxHp;
            this.currentMana = this.maxMana;
            this.currentSTR = this.strength;
            this.currentDEF = this.defense;
        }
    }

    public void AddExp(int amount)
    {
        currentExp += amount;
        if (currentExp >= requiredExp)
        {
            level++;
            UpdateStatsFromCSV(level);
            Debug.Log("Level Up: " + level);
        }
        SaveData();
    }

    public void SaveData()
    {
        FirestoreManager db = FindObjectOfType<FirestoreManager>();
        if (db != null)
        {
            db.SavePlayerStats(walletAddress, level, currentExp, currentHp, currentMana, strength, defense, transform.position);
        }
    }

    private void OnApplicationQuit()
    {
        SaveData();
    }
}