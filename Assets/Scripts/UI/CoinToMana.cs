using TMPro;
using UnityEngine;

public class CoinToMana : MonoBehaviour
{
    public CanvasGroup buyMana;
    public TextMeshProUGUI infor;
    public TextMeshProUGUI notE;
    public void BuyMana()
    {
        // Reset cả hai text trước khi kiểm tra
       

        if (PlayerStats.Instance.coin < 40)
        {
            notE.gameObject.SetActive(true);
            infor.gameObject.SetActive(false);

            return;
        }

        PlayerStats.Instance.currentMana += 40;
        PlayerStats.Instance.AddCoin(-40);

        PlayerHealth.instance.UpdateUI();

        buyMana.alpha = 0;
        buyMana.interactable = false;
        buyMana.blocksRaycasts = false;
    }

    public void ResetBtn()
    {
        infor.gameObject.SetActive(true);
        notE.gameObject.SetActive(false);
    }
}
