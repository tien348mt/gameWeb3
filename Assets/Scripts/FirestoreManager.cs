using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class FirestoreManager : MonoBehaviour
{
    private string projectId = "gamelord1-49c71";

    /********************************* PlayerStats *******************************/
    public void SavePlayerStats(string wallet, int level, int exp, float hp, float mana, float str, float def, Vector3 pos)
    {
        StartCoroutine(PatchStatsToFirestore(wallet, level, exp, hp, mana, str, def, pos));
    }

    IEnumerator PatchStatsToFirestore(string wallet, int level, int exp, float hp, float mana, float str, float def, Vector3 pos)
    {
        string cleanWallet = wallet.Trim();
        // Đảm bảo URL chính xác
        string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/Users/{cleanWallet}?updateMask.fieldPaths=stats";

        // Debug URL để kiểm tra xem ví có bị rỗng không
        Debug.Log("Đang gửi đến URL: " + url);

        string json = "{" +
            "\"fields\": {" +
                "\"stats\": {\"mapValue\": {\"fields\": {" +
                    "\"level\": {\"integerValue\": \"" + level + "\"}," +
                    "\"exp\": {\"integerValue\": \"" + exp + "\"}," +
                    "\"hp\": {\"doubleValue\": " + hp + "}," +
                    "\"mana\": {\"doubleValue\": " + mana + "}," +
                    "\"strength\": {\"doubleValue\": " + str + "}," +
                    "\"defense\": {\"doubleValue\": " + def + "}," +
                    "\"lastPosition\": {\"stringValue\": \"" + pos.x + "," + pos.y + "," + pos.z + "\"}" +
                "}}}}" +
            "}" +
        "}";

        using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                Debug.Log(">>> LƯU THÀNH CÔNG!");
            else
                Debug.LogError(">>> LỖI FIREBASE: " + request.downloadHandler.text);
        }
    }

    /********************************* Load PlayerStats *******************************/
    public delegate void OnStatsLoaded(int level, int exp, Vector3 position);

    public void LoadPlayerStats(string wallet, OnStatsLoaded callback)
    {
        StartCoroutine(GetStatsFromFirestore(wallet, callback));
    }

    IEnumerator GetStatsFromFirestore(string wallet, OnStatsLoaded callback)
    {
        string cleanWallet = wallet.Trim();
        string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/Users/{cleanWallet}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string text = request.downloadHandler.text;

                int lv = int.Parse(ExtractValue(text, "level", "integerValue"));
                int exp = int.Parse(ExtractValue(text, "exp", "integerValue"));
                string posStr = ExtractValue(text, "lastPosition", "stringValue");

                string[] p = posStr.Split(',');
                Vector3 pos = new Vector3(float.Parse(p[0]), float.Parse(p[1]), float.Parse(p[2]));

                callback?.Invoke(lv, exp, pos);
            }
            else
            {
                Debug.Log("Người chơi mới - Khởi tạo Level 1");
                callback?.Invoke(1, 0, Vector3.zero);
            }
        }
    }
    private string ExtractValue(string json, string fieldName, string type)
    {
        string search = "\"" + fieldName + "\": {\"" + type + "\": \"";
        int start = json.IndexOf(search) + search.Length;
        int end = json.IndexOf("\"", start);
        return json.Substring(start, end - start);
    }

    /*********************************Items*******************************/
    public void AddItemToInventory(string wallet, ItemData item)
    {
        StartCoroutine(PostToFirestore(wallet, item));
    }

    IEnumerator PostToFirestore(string wallet, ItemData item)
    {
        string projectId = "gamelord1-49c71";
        string cleanWallet = wallet.Trim();
        string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/Users/{cleanWallet}/Inventory";

        string json = "{" +
            "\"fields\": {" +
                "\"itemId\": {\"stringValue\": \"" + item.itemId + "\"}," +
                "\"itemName\": {\"stringValue\": \"" + item.itemName + "\"}," +
                "\"metadataUri\": {\"stringValue\": \"" + (item.metadataUri ?? "") + "\"}," +
                "\"basePrice\": {\"stringValue\": \"" + item.basePrice + "\"}," +
                "\"armor\": {\"stringValue\": \"" + item.armor + "\"}," +
                "\"attack\": {\"stringValue\": \"" + item.attack + "\"}," +
                "\"isMinted\": {\"booleanValue\": false}" +
            "}" +
        "}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log(">>> THÀNH CÔNG: Đã đẩy đầy đủ thuộc tính lên Firebase!");
            }
            else
            {
                Debug.LogError(">>> LỖI FIREBASE: " + request.downloadHandler.text);
            }
        }
    }
}