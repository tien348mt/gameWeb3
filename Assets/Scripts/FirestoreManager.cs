using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class FirestoreManager : MonoBehaviour
{
    private string projectId = "gamelord1-49c71"; // Thay bằng ID của bạn

    public void AddItemToInventory(string wallet, ItemData item)
    {
        StartCoroutine(PostToFirestore(wallet, item));
    }

    IEnumerator PostToFirestore(string wallet, ItemData item)
    {
        // 1. Dùng đúng ID từ ảnh của bạn
        string projectId = "gamelord1-49c71";

        // 2. Link này sẽ tạo item bên trong Inventory của ví đó
        // Lưu ý: Tôi dùng .Trim() để đảm bảo wallet không bị dính ký tự xuống dòng
        string cleanWallet = wallet.Trim();
        string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/Users/{cleanWallet}/Inventory";

        string json = "{\"fields\": {" +
            "\"itemId\": {\"stringValue\": \"" + item.itemId + "\"}," +
            "\"itemName\": {\"stringValue\": \"" + item.itemName + "\"}," +
            "\"isMinted\": {\"booleanValue\": false}" +
            "}}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log(">>> THÀNH CÔNG: Đã đẩy đồ lên Firebase!");
            }
            else
            {
                // Nếu vẫn lỗi, hãy đọc dòng này ở Console để biết tại sao
                Debug.LogError(">>> LỖI FIREBASE: " + request.downloadHandler.text);
            }
        }
    }
}