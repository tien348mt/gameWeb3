using UnityEngine;
using TMPro; // Bắt buộc để dùng TextMeshPro

public class Collectible : MonoBehaviour
{
    public ItemData data; // File ItemData của món đồ (Armor, Sword...)

    [Header("Cấu hình UI")]
    public TextMeshProUGUI walletText; // Chúng ta sẽ kéo thả đối tượng vào đây

    private async void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra nếu chạm vào Player
        if (other.CompareTag("Player"))
        {
            if (walletText != null)
            {
                // Lấy địa chỉ ví từ TextMeshPro
                // Thay vì đọc từ TextMeshPro, bạn có thể lấy trực tiếp từ SDK
                string wallet = await ThirdwebManager.Instance.SDK.wallet.GetAddress();

                // Tìm FirestoreManager để lưu dữ liệu
                FirestoreManager db = FindObjectOfType<FirestoreManager>();

                if (db != null)
                {
                    db.AddItemToInventory(wallet, data);
                    Debug.Log("Đã nhặt: " + data.itemName + " cho ví: " + wallet);

                    // Xóa item sau khi nhặt
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