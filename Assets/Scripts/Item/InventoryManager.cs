using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public string projectId = "gamelord1-49c71";
    public GameObject slotPrefab;
    public Transform contentParent;
    public CanvasGroup canvasGroup;
    public ItemDatabase database;
    public TextMeshProUGUI walletText;

    bool isInventoryOpen = false;
    public static InventoryManager Instance;

    /*void Awake() { Instance = this; }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isInventoryOpen = !isInventoryOpen;
            if (isInventoryOpen) OpenInventory();
            else CloseInventory();
            Debug.Log("open");
        }
    }

    public void OpenInventory()
    {
        if (walletText == null) return;
        string wallet = walletText.text.Trim();
        if (string.IsNullOrEmpty(wallet) || wallet.Length < 10) return;

        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        foreach (Transform child in contentParent) Destroy(child.gameObject);
        StartCoroutine(GetItems(wallet));
    }

    public void CloseInventory()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false; 
        isInventoryOpen = false; 
    }*/
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Phím B để mở/đóng (giữ nguyên)
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleInventory();
        }
    }

    // ====================== TOGGLE INVENTORY (NÚT BUTTON SẼ GỌI HÀM NÀY) ======================
    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;

        if (isInventoryOpen)
            OpenInventory();
        else
            CloseInventory();
    }

    public void OpenInventory()
    {
        Debug.Log("=== OpenInventory() được gọi ===");

        if (canvasGroup == null)
        {
            Debug.LogError("❌ CanvasGroup chưa được gán trong InventoryManager!");
            return;
        }

        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // Xóa slot cũ
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        // Nếu chưa có wallet thì vẫn mở để test
        string wallet = (walletText != null) ? walletText.text.Trim() : "";
        if (string.IsNullOrEmpty(wallet) || wallet.Length < 10)
        {
            Debug.LogWarning("⚠️ Wallet chưa có hoặc chưa connect, vẫn mở inventory để test");
        }

        StartCoroutine(GetItems(wallet));
        Debug.Log("✅ Inventory UI đã mở");
    }

    public void CloseInventory()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        isInventoryOpen = false;
        Debug.Log("✅ Inventory UI đã đóng");
    }
    IEnumerator GetItems(string wallet)
    {
        string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/Users/{wallet}/Inventory";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Lỗi tải Inventory: " + request.error);
                yield break;
            }

            string json = request.downloadHandler.text;
            
            string[] docs = json.Split(new string[] { "\"name\": \"projects/" }, System.StringSplitOptions.None);

            for (int i = 1; i < docs.Length; i++)
            {
                string docContent = docs[i];


                // 1. Lấy docId
                string[] nameSplit = docContent.Split('"');
                string fullPath = nameSplit[0];
                string[] pathParts = fullPath.Split('/');
                string docId = pathParts[pathParts.Length - 1];

                if (docContent.Contains("\"itemId\""))
                {
                    string[] splitId = docContent.Split(new string[] { "\"itemId\":" }, System.StringSplitOptions.None);
                    string itemId = splitId[1].Split(new string[] { "\"stringValue\": \"" }, System.StringSplitOptions.None)[1].Split('"')[0];

                    Debug.Log(">>> Đã tìm thấy ItemId từ Firebase: " + itemId);

                    ItemData originalData = database.GetItemById(itemId);
                    if (originalData != null)
                    {
                        ItemData displayData = Instantiate(originalData);
                        displayData.armor = GetFieldValue(docContent, "armor");
                        displayData.attack = GetFieldValue(docContent, "attack");
                        displayData.hp = GetFieldValue(docContent, "hp");
                        displayData.mana = GetFieldValue(docContent, "mana");
                        displayData.basePrice = GetFieldValue(docContent, "basePrice");

                        GameObject slot = Instantiate(slotPrefab, contentParent);
                        slot.GetComponent<InventorySlotUI>().Setup(displayData, walletText, docId);
                        Debug.Log(">>> Đã tạo xong slot cho: " + itemId + " aaa "+ docId);
                    }
                }
            }
        }
    }

    private string GetFieldValue(string content, string fieldName)
    {
        if (!content.Contains("\"" + fieldName + "\"")) return "";
        string[] splitField = content.Split(new string[] { "\"" + fieldName + "\":" }, System.StringSplitOptions.None);
        if (splitField.Length < 2) return "";
        
        // Tách lấy giá trị sau stringValue
        string[] valSplit = splitField[1].Split(new string[] { "\"stringValue\": \"" }, System.StringSplitOptions.None);
        if (valSplit.Length < 2) return "";
        
        return valSplit[1].Split('"')[0];
    }
}