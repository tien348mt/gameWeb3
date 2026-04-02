using UnityEngine;
using TMPro;

public class Collectible : MonoBehaviour
{
    public ItemData data;

    [Header("Cấu hình UI")]
    public TextMeshProUGUI walletText;

    private async void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (walletText != null)
            {
                string wallet = await ThirdwebManager.Instance.SDK.wallet.GetAddress();

                FirestoreManager db = FindObjectOfType<FirestoreManager>();

                if (db != null)
                {
                    db.AddItemToInventory(wallet, data);
                    Debug.Log("Đã nhặt: " + data.itemName + " cho ví: " + wallet);

                    Destroy(gameObject);
                }
                else
                {
                    Debug.LogError("Lỗi: Không tìm thấy FirestoreManager trong Scene!");
                }
            }
            else
            {
                Debug.LogError("Lỗi: Bạn chưa kéo thả Wallet Text vào ô Wallet Text trong Inspector!");
            }
        }
    }
}