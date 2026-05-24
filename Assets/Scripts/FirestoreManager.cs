using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class FirestoreManager : MonoBehaviour
{
    public static FirestoreManager Instance;
    private string projectId = "gamelord1-49c71";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    /********************************* PlayerStats *******************************/
    public void SavePlayerStats(string wallet, int level, int exp, float hp, float mana, float str, float def, float maxHP, float maxMana, Vector3 pos)
    {
        StartCoroutine(PatchStatsToFirestore(wallet, level, exp, hp, mana, str, def, maxHP, maxMana, pos));
    }

    IEnumerator PatchStatsToFirestore(string wallet, int level, int exp, float hp, float mana, float str, float def, float maxHP, float maxMana, Vector3 pos)
    {
        string cleanWallet = wallet.Trim();
        string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/Users/{cleanWallet}?updateMask.fieldPaths=stats";

        string hpS = hp.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        string manaS = mana.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        string strS = str.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        string defS = def.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        string maxhpS = maxHP.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        string maxmanaS = maxMana.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
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
                            "\"maxHP\": {\"doubleValue\": " + maxhpS + "}," +
                            "\"maxMana\": {\"doubleValue\": " + maxmanaS + "}," +
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
    public delegate void OnStatsLoaded(int level, int exp, float hp, float mana, float str, float def, float maxHP, float maxMana, Vector3 position);

    public void LoadPlayerStats(string wallet, OnStatsLoaded callback)
    {
        StartCoroutine(GetStatsFromFirestore(wallet, callback));
    }

    IEnumerator GetStatsFromFirestore(string wallet, OnStatsLoaded callback)
    {
        string cleanWallet = wallet.Trim();

        string url =
            $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/Users/{cleanWallet}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.responseCode == 404)
            {
                Debug.Log(">>> Không tìm thấy nhân vật, tạo mới");

                callback?.Invoke(1, 0, 20f, 15f, 5f, 15f, 20f, 15f, Vector3.zero);
                SavePlayerStats(wallet, 1, 0, 20f, 15f, 10f, 5f, 20f, 15f, Vector3.zero);

                yield break;
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                string text = request.downloadHandler.text;

                if (!text.Contains("\"fields\""))
                {
                    Debug.Log(">>> Không có dữ liệu -> tạo nhân vật mới");

                    callback?.Invoke(1, 0, 20f, 15f, 10f, 5f, 20f, 15f, Vector3.zero);

                    SavePlayerStats(wallet, 1, 0, 20f, 15f, 10f, 5f, 20f, 15f, Vector3.zero);

                    yield break;
                }

                int lv = 1;
                int exp = 0;
                float hp = 0;
                float mana = 0;
                float str = 0;
                float def = 0;
                float maxHP = 0;
                float maxMana = 0;
                Vector3 pos = Vector3.zero;

                string lvStr = ExtractValue(text, "level", "integerValue");
                string expStr = ExtractValue(text, "exp", "integerValue");

                string hpStr = ExtractValue(text, "hp", "doubleValue") ?? ExtractValue(text, "hp", "integerValue");
                string manaStr = ExtractValue(text, "mana", "doubleValue") ?? ExtractValue(text, "mana", "integerValue");
                string strStr = ExtractValue(text, "strength", "doubleValue") ?? ExtractValue(text, "strength", "integerValue");
                string defStr = ExtractValue(text, "defense", "doubleValue") ?? ExtractValue(text, "defense", "integerValue");
                string maxhpStr = ExtractValue(text, "maxHP", "doubleValue") ?? ExtractValue(text, "maxHP", "integerValue");
                string maxmanaStr = ExtractValue(text, "maxMana", "doubleValue") ?? ExtractValue(text, "maxMana", "integerValue");

                string posStr = ExtractValue(text, "lastPosition", "stringValue");

                if (!string.IsNullOrEmpty(lvStr)) int.TryParse(lvStr, out lv);
                if (!string.IsNullOrEmpty(expStr)) int.TryParse(expStr, out exp);

                hp = ParseFloatSafe(hpStr);
                mana = ParseFloatSafe(manaStr);
                str = ParseFloatSafe(strStr);
                def = ParseFloatSafe(defStr);
                maxHP = ParseFloatSafe(maxhpStr);
                maxMana = ParseFloatSafe(maxmanaStr);

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

                Debug.Log($">>> LOAD NHÂN VẬT: Lv {lv}, Exp {exp}, Pos {pos}");
                callback?.Invoke(lv, exp, hp, mana, str, def, maxHP, maxMana, pos);
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

    private float ParseFloatSafe(string value)
    {
        if (string.IsNullOrEmpty(value)) return 0;
        float result;
        if (float.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result))
        {
            return result;
        }

        return 0;
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
                "\"hp\": {\"stringValue\": \"" + item.hp + "\"}," +
                "\"mana\": {\"stringValue\": \"" + item.mana + "\"}," +
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



    /********************************* QUEST SYSTEM - LƯU THEO WALLET ID *******************************/

    public void SavePlayerQuests(string wallet, List<QuestManager.QuestProgress> activeQuests, List<string> completedQuests, List<string> destroyedObjects)
    {
        StartCoroutine(PatchQuestsToFirestore(wallet, activeQuests, completedQuests, destroyedObjects));
    }

    IEnumerator PatchQuestsToFirestore(string wallet, List<QuestManager.QuestProgress> active, List<string> completed, List<string> destroyed)
    {
        string cleanWallet = wallet.Trim();

        string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/Users/{cleanWallet}" +
                     "?updateMask.fieldPaths=activeQuests" +
                     "&updateMask.fieldPaths=completedQuests" +
                     "&updateMask.fieldPaths=permanentlyDestroyedObjects";

        string activeFields = "";

        foreach (var q in active)
        {
            string objs = string.Join(",", q.completedObjectives.ConvertAll(o =>
                "{\"stringValue\": \"" + o + "\"}"));

            activeFields += "\"" + q.questID +
                            "\": {\"mapValue\": {\"fields\": {" +
                            "\"completedObjectives\": {\"arrayValue\": {\"values\": [" + objs + "]}}," +
                            "\"isCompleted\": {\"booleanValue\": " + q.isCompleted.ToString().ToLower() + "}" +
                            "}}},";
        }

        if (activeFields.EndsWith(",")) activeFields = activeFields.TrimEnd(',');

        string completedArray = string.Join(",", completed.ConvertAll(c =>
            "{\"stringValue\": \"" + c + "\"}"));

        string destroyedArray = string.Join(",", destroyed.ConvertAll(d =>
            "{\"stringValue\": \"" + d + "\"}"));

        string json =
        "{" +
            "\"fields\": {" +
                "\"activeQuests\": {\"mapValue\": {\"fields\": {" + activeFields + "}}}," +
                "\"completedQuests\": {\"arrayValue\": {\"values\": [" + completedArray + "]}}," +
                "\"permanentlyDestroyedObjects\": {\"arrayValue\": {\"values\": [" + destroyedArray + "]}}" +
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
                Debug.Log("<color=green>>>> QUEST DATA SAVED TO FIREBASE!</color>");
            else
                Debug.LogError(">>> QUEST SAVE ERROR: " + request.downloadHandler.text);
        }
    }

    public delegate void OnQuestsLoaded(List<QuestManager.QuestProgress> activeQuests, List<string> completedQuests, List<string> destroyedObjects);

    public void LoadPlayerQuests(string wallet, OnQuestsLoaded callback)
    {
        StartCoroutine(GetQuestsFromFirestore(wallet, callback));
    }

    /* IEnumerator GetQuestsFromFirestore(string wallet, OnQuestsLoaded callback)
     {
         string cleanWallet = wallet.Trim();

         string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/Users/{cleanWallet}";

         using (UnityWebRequest request = UnityWebRequest.Get(url))
         {
             yield return request.SendWebRequest();

             if (request.result == UnityWebRequest.Result.Success)
             {
                 string text = request.downloadHandler.text;

                 List<QuestManager.QuestProgress> active = new List<QuestManager.QuestProgress>();
                 List<string> completed = new List<string>();
                 List<string> destroyed = new List<string>();

                 Match compMatch = Regex.Match(
                     text,
                     @"""completedQuests""\s*:\s*\{[^}]*""values""\s*:\s*\[(.*?)\]",
                     RegexOptions.Singleline);

                 if (compMatch.Success)
                 {
                     MatchCollection ids = Regex.Matches(
                         compMatch.Groups[1].Value,
                         @"""stringValue""\s*:\s*""([^""]+)""");

                     foreach (Match m in ids)
                         completed.Add(m.Groups[1].Value);
                 }

                 Match destMatch = Regex.Match(
                     text,
                     @"""permanentlyDestroyedObjects""\s*:\s*\{[^}]*""values""\s*:\s*\[(.*?)\]",
                     RegexOptions.Singleline);

                 if (destMatch.Success)
                 {
                     MatchCollection ids = Regex.Matches(
                         destMatch.Groups[1].Value,
                         @"""stringValue""\s*:\s*""([^""]+)""");

                     foreach (Match m in ids)
                         destroyed.Add(m.Groups[1].Value);
                 }

                 Match activeMatch = Regex.Match(
                     text,
                     @"""activeQuests""\s*:\s*\{[^}]*""fields""\s*:\s*\{(.*?)\}\s*\}",
                     RegexOptions.Singleline);

                 if (activeMatch.Success)
                 {
                     string fieldsStr = activeMatch.Groups[1].Value;

                     MatchCollection questMatches = Regex.Matches(
                         fieldsStr,
                         @"""([^""]+)""\s*:\s*\{[^}]*completedObjectives[^[]*\[(.*?)\][^}]*booleanValue""\s*:\s*(true|false)",
                         RegexOptions.Singleline);

                     foreach (Match m in questMatches)
                     {
                         QuestManager.QuestProgress qp = new QuestManager.QuestProgress
                         {
                             questID = m.Groups[1].Value,
                             isCompleted = m.Groups[3].Value.ToLower() == "true"
                         };

                         string objStr = m.Groups[2].Value;

                         MatchCollection objMatches = Regex.Matches(
                             objStr,
                             @"""stringValue""\s*:\s*""([^""]+)""");

                         foreach (Match om in objMatches)
                             qp.completedObjectives.Add(om.Groups[1].Value);

                         active.Add(qp);
                     }
                 }

                 callback?.Invoke(active, completed, destroyed);
             }
             else
             {
                 callback?.Invoke(new List<QuestManager.QuestProgress>(), new List<string>(), new List<string>());
             }
         }
     }*/

    // Thay toàn bộ IEnumerator GetQuestsFromFirestore bằng cái này

    IEnumerator GetQuestsFromFirestore(string wallet, OnQuestsLoaded callback)
    {
        string cleanWallet = wallet.Trim();
        string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/Users/{cleanWallet}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            List<QuestManager.QuestProgress> active = new List<QuestManager.QuestProgress>();
            List<string> completed = new List<string>();
            List<string> destroyed = new List<string>();

            if (request.result != UnityWebRequest.Result.Success)
            {
                callback?.Invoke(active, completed, destroyed);
                yield break;
            }

            string text = request.downloadHandler.text;

            // ── completedQuests ──────────────────────────────────
            Match compMatch = Regex.Match(text,
                @"""completedQuests""\s*:\s*\{.*?""values""\s*:\s*\[(.*?)\]",
                RegexOptions.Singleline);

            if (compMatch.Success)
                foreach (Match m in Regex.Matches(compMatch.Groups[1].Value,
                    @"""stringValue""\s*:\s*""([^""]+)"""))
                    completed.Add(m.Groups[1].Value);

            // ── permanentlyDestroyedObjects ──────────────────────
            Match destMatch = Regex.Match(text,
                @"""permanentlyDestroyedObjects""\s*:\s*\{.*?""values""\s*:\s*\[(.*?)\]",
                RegexOptions.Singleline);

            if (destMatch.Success)
                foreach (Match m in Regex.Matches(destMatch.Groups[1].Value,
                    @"""stringValue""\s*:\s*""([^""]+)"""))
                    destroyed.Add(m.Groups[1].Value);

            // ── activeQuests ─────────────────────────────────────
            // Tìm từng questID bên trong activeQuests.mapValue.fields
            Match activeBlock = Regex.Match(text,
                @"""activeQuests""\s*:\s*\{.*?""mapValue""\s*:\s*\{.*?""fields""\s*:\s*\{(.*)\}\s*\}\s*\}",
                RegexOptions.Singleline);

            if (activeBlock.Success)
            {
                string fieldsStr = activeBlock.Groups[1].Value;

                // Tìm từng quest block: "questID": { mapValue: { fields: { ... } } }
                MatchCollection questBlocks = Regex.Matches(fieldsStr,
                    @"""([^""]+)""\s*:\s*\{.*?""mapValue""\s*:\s*\{.*?""fields""\s*:\s*\{(.*?)\}\s*\}\s*\}",
                    RegexOptions.Singleline);

                foreach (Match qm in questBlocks)
                {
                    string questID = qm.Groups[1].Value;
                    string questField = qm.Groups[2].Value;

                    var qp = new QuestManager.QuestProgress { questID = questID };

                    // completedObjectives
                    Match objArr = Regex.Match(questField,
                        @"""completedObjectives""\s*:\s*\{.*?""values""\s*:\s*\[(.*?)\]",
                        RegexOptions.Singleline);

                    if (objArr.Success)
                        foreach (Match om in Regex.Matches(objArr.Groups[1].Value,
                            @"""stringValue""\s*:\s*""([^""]+)"""))
                            qp.completedObjectives.Add(om.Groups[1].Value);

                    // isCompleted
                    Match isDone = Regex.Match(questField,
                        @"""isCompleted""\s*:\s*\{.*?""booleanValue""\s*:\s*(true|false)",
                        RegexOptions.Singleline);

                    if (isDone.Success)
                        qp.isCompleted = isDone.Groups[1].Value == "true";

                    active.Add(qp);
                    Debug.Log($"[Load] Quest '{questID}' — objectives: [{string.Join(", ", qp.completedObjectives)}] — done: {qp.isCompleted}");
                }
            }

            callback?.Invoke(active, completed, destroyed);
        }
    }

    // ====================== LƯU VỊ TRÍ NPC KHI HOÀN THÀNH QUEST ======================

    public void SaveNPCPosition(string wallet, string npcID, Vector3 position)
    {
        StartCoroutine(PatchNPCPosition(wallet, npcID, position));
    }

    IEnumerator PatchNPCPosition(string wallet, string npcID, Vector3 pos)
    {
        string cleanWallet = wallet.Trim();

        string url =
            $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/Users/{cleanWallet}" +
            "?updateMask.fieldPaths=npcPositions";

        string posStr =
            pos.x.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + "," +
            pos.y.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + "," +
            pos.z.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

        string json =
        "{" +
            "\"fields\":{" +
                "\"npcPositions\":{" +
                    "\"mapValue\":{" +
                        "\"fields\":{" +
                            $"\"{npcID}\":{{\"stringValue\":\"{posStr}\"}}" +
                        "}" +
                    "}" +
                "}" +
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
                Debug.Log($"✅ Đã lưu vị trí NPC '{npcID}' = {pos}");
            else
                Debug.LogError("Lỗi lưu NPC: " + request.downloadHandler.text);
        }
    }


    // ====================== LOAD VỊ TRÍ NPC TỪ FIREBASE ======================

    public void LoadNPCPosition(string wallet, string npcID, System.Action<Vector3> callback)
    {
        StartCoroutine(GetNPCPosition(wallet, npcID, callback));
    }

    IEnumerator GetNPCPosition(string wallet, string npcID, System.Action<Vector3> callback)
    {
        string cleanWallet = wallet.Trim();

        string url =
            $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/Users/{cleanWallet}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;

                // TÌM npcID TRONG npcPositions
                string pattern =
                    $"\"{npcID}\"\\s*:\\s*\\{{\\s*\"stringValue\"\\s*:\\s*\"([^\"]+)\"";

                Match match = Regex.Match(json, pattern);

                if (match.Success)
                {
                    string posStr = match.Groups[1].Value;
                    string[] parts = posStr.Split(',');

                    if (parts.Length == 3)
                    {
                        if (float.TryParse(parts[0], System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out float x) &&
                            float.TryParse(parts[1], System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out float y) &&
                            float.TryParse(parts[2], System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out float z))
                        {
                            Vector3 pos = new Vector3(x, y, z);

                            Debug.Log($"Load NPC '{npcID}' = {pos}");
                            callback?.Invoke(pos);
                            yield break;
                        }
                    }
                }
            }

            Debug.LogWarning($"NPC '{npcID}' chưa có vị trí dùng mặc định");
            callback?.Invoke(Vector3.zero);
        }
    }


    // ====================== COIN ======================

    public void SaveCoin(string wallet, int coin)
    {
        StartCoroutine(PatchCoinToFirestore(wallet, coin));
    }

    IEnumerator PatchCoinToFirestore(string wallet, int coin)
    {
        string cleanWallet = wallet.Trim();
        string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/Users/{cleanWallet}?updateMask.fieldPaths=coin";

        string json = "{\"fields\": {\"coin\": {\"integerValue\": \"" + coin + "\"}}}";

        using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                Debug.Log($"✅ Đã lưu coin: {coin}");
            else
                Debug.LogError("Lỗi lưu coin: " + request.downloadHandler.text);
        }
    }

    public void LoadCoin(string wallet, System.Action<int> callback)
    {
        StartCoroutine(GetCoinFromFirestore(wallet, callback));
    }

    IEnumerator GetCoinFromFirestore(string wallet, System.Action<int> callback)
    {
        string cleanWallet = wallet.Trim();
        string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/Users/{cleanWallet}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            int coin = 0;

            if (request.result == UnityWebRequest.Result.Success)
            {
                string text = request.downloadHandler.text;
                string coinStr = ExtractValue(text, "coin", "integerValue");
                if (!string.IsNullOrEmpty(coinStr))
                    int.TryParse(coinStr, out coin);
            }

            Debug.Log($"✅ Load coin: {coin}");
            callback?.Invoke(coin);
        }
    }
}