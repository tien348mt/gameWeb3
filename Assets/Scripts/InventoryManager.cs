using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public string projectId = "gamelord1-49c71";
    public GameObject slotPrefab;
    public Transform contentParent;
    public GameObject inventory;
    public ItemDatabase database;
    public TextMeshProUGUI walletText;

    bool isInventoryOpen = false;
    public static InventoryManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            isInventoryOpen = !isInventoryOpen;

            if (isInventoryOpen)
                OpenInventory();
            else
                CloseInventory();
        }
    }

    public void OpenInventory()
    {
        if (walletText == null) return;

        // Lấy địa chỉ ví hiện tại từ UI
        string wallet = walletText.text.Trim();

        // Kiểm tra nếu ví chưa được load (tránh lỗi gửi request trống)
        if (string.IsNullOrEmpty(wallet) || wallet.Length < 10)
        {
            Debug.LogWarning("Ví chưa sẵn sàng!");
            return;
        }

        inventory.SetActive(true);

        // Xóa các ô cũ trước khi tải dữ liệu mới
        foreach (Transform child in contentParent) Destroy(child.gameObject);

        StartCoroutine(GetItems(wallet));
    }

    public void CloseInventory()
    {
        inventory.SetActive(false);
        isInventoryOpen = false;
    }

    IEnumerator GetItems(string wallet)
    {
        string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/Users/{wallet}/Inventory";
        Debug.Log("Đang kết nối: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Dữ liệu về: " + jsonResponse);

                // 1. Regex lấy Document ID (Để xóa khi bán)
                // Lưu ý: Regex này lấy phần ID cuối cùng sau dấu gạch chéo cuối cùng của trường "name"
                MatchCollection idMatches = Regex.Matches(jsonResponse, @"\""name\"": \""projects/[^/]+/databases/\(default\)/documents/Users/[^/]+/Inventory/([^\""]+)\""");

                // 2. Regex lấy ItemId (Lấy chính xác giá trị stringValue của trường itemId)
                // Cải tiến Regex để bắt được itemId ngay cả khi cấu trúc Document thay đổi
                MatchCollection itemMatches = Regex.Matches(jsonResponse, @"\""itemId\"":\s*\{\s*\""stringValue\"":\s*\""([^\""]+)\""");

                for (int i = 0; i < itemMatches.Count; i++)
                {
                    // Kiểm tra đảm bảo có đủ cả ID và dữ liệu item
                    if (i < idMatches.Count)
                    {
                        string docId = idMatches[i].Groups[1].Value;
                        string itemId = itemMatches[i].Groups[1].Value;

                        ItemData data = database.GetItemById(itemId);
                        if (data != null)
                        {
                            GameObject slot = Instantiate(slotPrefab, contentParent);
                            // Setup ô đồ với dữ liệu, ví người sở hữu và mã Document trên Firebase
                            slot.GetComponent<InventorySlotUI>().Setup(data, walletText, docId);

                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Lỗi tải Inventory: " + request.error);
            }
        }
    }
}