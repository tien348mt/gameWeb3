using UnityEngine;
using System.Threading.Tasks;
using System.Collections;
using DG.Tweening;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;
    public GameObject levelup;

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

    [Header("Currency")]
    public int coin = 0;

    [SerializeField] private RespawnManager respawnManager;

   
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
        if (levelup != null) levelup.SetActive(false);
    }

    private void Update()
    {
        
    }
    public void LoadPlayerData()
    {
        FirestoreManager db = FindObjectOfType<FirestoreManager>();
        if (db != null)
        {
            db.LoadPlayerStats(walletAddress, (lv,exp, hp, mana, str, def,maxHP, maxMana, pos) =>
            {
                this.level = lv;
                this.currentExp = exp;
                StartCoroutine(WaitAndUpdateStats(lv, pos));
                this.currentHp = hp;
                this.currentMana = mana;
                this.strength = str;
                this.defense = def;
                this.maxHp = maxHP;
                this.maxMana = maxMana;
            });

            db.LoadCoin(walletAddress, (c) => { this.coin = c; CoinUI.Instance.UpdateUI(); });
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
            this.maxHp += data.HP;
            this.maxMana += data.MANA;
            this.strength += data.STR;
            this.defense += data.DEF;
            this.requiredExp = data.EXP;

            this.currentHp = this.maxHp;
            this.currentMana = this.maxMana;
            /*this.currentSTR = this.strength;
            this.currentDEF = this.defense;*/
        }
    }

    public void AddExp(int amount)
    {
        currentExp += amount;
        while (currentExp >= requiredExp)
        {
            currentExp -= requiredExp;
            level++;
            UpdateStatsFromCSV(level);
            Debug.Log("Level Up: " + level);
            TriggerLevelUpAnimation();


        }
        SaveData();
    }

    private void TriggerLevelUpAnimation()
    {
        if (levelup == null) return;

        levelup.transform.DOKill();
        levelup.SetActive(true);

        // Luôn giữ scale dương để không bị flip
        levelup.transform.localScale = Vector3.zero;

        Vector3 targetScale = new Vector3(0.5f, 0.5f, 0.5f);

        levelup.transform
            .DOScale(targetScale, 0.5f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                DOVirtual.DelayedCall(0.7f, () =>
                {
                    levelup.transform
                        .DOScale(Vector3.zero, 0.2f)
                        .SetEase(Ease.InBack)
                        .OnComplete(() => levelup.SetActive(false));
                });
            });
    }
    public void AddCoin(int amount)
    {
        coin += amount;
        CoinUI.Instance.UpdateUI();
        SaveData();
    }
    public void SaveData()
    {
        FirestoreManager db = FindObjectOfType<FirestoreManager>();
        if (db != null)
        {
            db.SavePlayerStats(walletAddress, level, currentExp, currentHp, currentMana, strength, defense, maxHp, maxMana, transform.position);
            LoadStatsPlayer.instance.Information();
            PlayerHealth.instance.UplevelInformationUI();
            db.SaveCoin(walletAddress, coin);
        }
    }

    public void PlayerDead()
    {
        if(currentHp <= 0)
        {
            respawnManager.Respawn();
        }
    }

    private void OnApplicationQuit()
    {
        SaveData();
    }
}