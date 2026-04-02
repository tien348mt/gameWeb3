using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;

public class MarketplaceManager : MonoBehaviour
{
    public string projectId = "gamelord1-49c71";
    public GameObject shopSlotPrefab;
    public Transform shopContentParent;
    public ItemDatabase database;
    public GameObject shop;
    public TextMeshProUGUI walletText;

    bool isShopOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            isShopOpen = !isShopOpen;
            if (isShopOpen) OpenShop();
            else CloseShop();
        }
    }

    public void OpenShop()
    {
        shop.SetActive(true);
        StartCoroutine(LoadMarketplace());
    }

    public void CloseShop() => shop.SetActive(false);

    IEnumerator LoadMarketplace()
    {
        string url = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/Marketplace";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                foreach (Transform child in shopContentParent) Destroy(child.gameObject);

                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Dữ liệu nhận được: " + jsonResponse);

               
                MatchCollection idMatches = Regex.Matches(jsonResponse, @"\""name\"":\s*\""projects/[^/]+/databases/\(default\)/documents/Marketplace/([^\""]+)\""");
                MatchCollection items = Regex.Matches(jsonResponse, @"\""itemId\"":\s*\{\s*\""stringValue\"":\s*\""([^\""]+)\""");
                MatchCollection prices = Regex.Matches(jsonResponse, @"\""price\"":\s*\{\s*\""stringValue\"":\s*\""([^\""]+)\""");
                MatchCollection sellers = Regex.Matches(jsonResponse, @"\""seller\"":\s*\{\s*\""stringValue\"":\s*\""([^\""]+)\""");
                MatchCollection tokenIds = Regex.Matches(jsonResponse, @"\""tokenId\"":\s*\{\s*\""stringValue\"":\s*\""([^\""]+)\""");

                string currentMyWallet = (walletText != null) ? walletText.text.Trim() : "";

                
                int count = items.Count;
                Debug.Log($"Tìm thấy {count} vật phẩm hợp lệ trên Marketplace.");

                for (int i = 0; i < count; i++)
                {
                    if (i >= idMatches.Count || i >= prices.Count || i >= sellers.Count || i >= tokenIds.Count)
                    {
                        Debug.LogWarning($"Vật phẩm thứ {i} bị thiếu dữ liệu trường nào đó trên Firebase.");
                        continue;
                    }

                    string id = items[i].Groups[1].Value;
                    string price = prices[i].Groups[1].Value;
                    string seller = sellers[i].Groups[1].Value;
                    string mDocId = idMatches[i].Groups[1].Value;
                    string tId = tokenIds[i].Groups[1].Value;

                    ItemData data = database.GetItemById(id);
                    if (data != null)
                    {
                        GameObject slot = Instantiate(shopSlotPrefab, shopContentParent);
                        ShopSlotUI uiScript = slot.GetComponent<ShopSlotUI>();
                        if (uiScript != null)
                        {
                            uiScript.Setup(data, price, seller, mDocId, currentMyWallet, tId);
                        }
                    }
                    else
                    {
                        Debug.LogError($"Không tìm thấy ItemData cho ID: {id} trong Database.");
                    }
                }
            }
            else
            {
                Debug.LogError("Lỗi khi tải Marketplace: " + request.error);
            }
        }
    }
}