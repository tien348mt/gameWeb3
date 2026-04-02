using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;

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
        string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/Users/{cleanWallet}?updateMask.fieldPaths=stats";

        string hpS = hp.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        string manaS = mana.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        string strS = str.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        string defS = def.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        string posS = $"{pos.x.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)},{pos.y.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)},{pos.z.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}";
        
        string json = "{" +
            "\"fields\": {" +
                "\"stats\": {" +
                    "\"mapValue\": {" +
                        "\"fields\": {" +
                            "\"level\": {\"integerValue\": \"" + level + "\"}," +
                            "\"exp\": {\"integerValue\": \"" + exp + "\"}," +
                            "\"hp\": {\"doubleValue\": " + hpS + "}," +
                            "\"mana\": {\"doubleValue\": " + manaS + "}," +
                            "\"strength\": {\"doubleValue\": " + strS + "}," +
                            "\"defense\": {\"doubleValue\": " + defS + "}," +
                            "\"lastPosition\": {\"stringValue\": \"" + posS + "\"}" +
                        "}" +
                    "}" +
                "}" +
            "}" +
        "}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            request.method = "PATCH";

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

                int lv = 1;
                int exp = 0;
                Vector3 pos = Vector3.zero;

                string lvStr = ExtractValue(text, "level", "integerValue");
                string expStr = ExtractValue(text, "exp", "integerValue");
                string posStr = ExtractValue(text, "lastPosition", "stringValue");

                if (!string.IsNullOrEmpty(lvStr)) int.TryParse(lvStr, out lv);
                if (!string.IsNullOrEmpty(expStr)) int.TryParse(expStr, out exp);

                if (!string.IsNullOrEmpty(posStr) && posStr.Contains(","))
                {
                    string[] p = posStr.Split(',');
                    if (p.Length >= 3)
                    {
                        float.TryParse(p[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out pos.x);
                        float.TryParse(p[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out pos.y);
                        float.TryParse(p[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out pos.z);
                    }
                }
                if (lv <= 0) lv = 1;

                Debug.Log($">>> KẾT QUẢ: Lv {lv}, Exp {exp}, Pos {pos}");
                callback?.Invoke(lv, exp, pos);
            }
            else
            {
                callback?.Invoke(1, 0, Vector3.zero);
                SavePlayerStats(wallet, 1, 0, 20f, 15f, 10f, 5f, Vector3.zero);
            }
        }
    }
    private string ExtractValue(string json, string fieldName, string type)
    {
        string pattern = $"\"{fieldName}\"\\s*:\\s*\\{{[^\\}}]*?\"{type}\"\\s*:\\s*\"?([^\"\\}}]+)\"?";

        Match match = Regex.Match(json, pattern, RegexOptions.Singleline);

        if (match.Success)
            return match.Groups[1].Value.Trim();

        return null;
    }

    /*********************************POST Items*******************************/
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