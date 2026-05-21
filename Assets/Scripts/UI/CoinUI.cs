using UnityEngine;
using TMPro;

public class CoinUI : MonoBehaviour
{
    public static CoinUI Instance;

    [SerializeField] private TextMeshProUGUI coinText;

    public CanvasGroup buyMana;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (PlayerStats.Instance == null) return;
        coinText.text = PlayerStats.Instance.coin.ToString();
    }

    public void BuyMana()
    {
        buyMana.alpha = 1;
        buyMana.interactable = true;
        buyMana.blocksRaycasts = true;
    }
    public void CloseBuyMana()
    {
        buyMana.alpha = 0;
        buyMana.interactable = false;
        buyMana.blocksRaycasts = false;
    }
}