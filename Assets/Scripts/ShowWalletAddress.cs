using UnityEngine;
using TMPro;
using Thirdweb;

public class ShowWalletAddress : MonoBehaviour
{
    public TMP_Text walletText;

    async void Start()
    {
       ShowAddress();
    }

    public async void ShowAddress()
    {
        try
        {
            string address = await ThirdwebManager.Instance.SDK.wallet.GetAddress();

            if (!string.IsNullOrEmpty(address))
            {
                walletText.text = address;
                Debug.Log("Wallet Address: " + address);
            }
            else
            {
                walletText.text = "Chưa connect ví";
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
            walletText.text = "Lỗi lấy ví";
        }
    }
}