using UnityEngine;
using TMPro;
using Thirdweb;

public class ShowWalletAddress : MonoBehaviour
{
    public static ShowWalletAddress Instance;
    public TMP_Text walletText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
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