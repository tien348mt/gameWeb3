using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using System.Text;

public class ShopSlotUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI priceText;
    public Text buttonText;
    public Button actionButton;

    private ItemData itemData;
    private string sellerAddress;
    private string marketDocId;
    private string myWallet;
    private string priceETH;
    private string tokenId;

    public void Setup(ItemData data, string price, string seller, string mDocId, string currentWallet, string tId)
    {
        itemData = data;
        priceETH = price;
        sellerAddress = seller.Trim().ToLower();
        marketDocId = mDocId;
        myWallet = currentWallet.Trim().ToLower();
        tokenId = tId;

        iconImage.sprite = data.itemIcon;
        priceText.text = price + " ETH";

        actionButton.onClick.RemoveAllListeners();

        if (sellerAddress == myWallet)
        {
            buttonText.text = "HOÀN";
            actionButton.onClick.AddListener(() => StartCoroutine(ReturnItem()));
        }
        else
        {
            buttonText.text = "MUA";
            actionButton.onClick.AddListener(OnBuyClick);
        }
    }

    public async void OnBuyClick()
    {
        actionButton.interactable = false;
        buttonText.text = "WAIT...";

        // 1. Gọi Blockchain thực hiện mua NFT
        string result = await Web3Manager.Instance.BuyNFT(tokenId, priceETH);

        if (result == "success")
        {
            Debug.Log("Mua thành công trên Blockchain! Đang cập nhật Firebase...");
            StartCoroutine(TransferOwnership());
        }
        else
        {
            actionButton.interactable = true;
            buttonText.text = "MUA";
        }
    }

    IEnumerator TransferOwnership()
    {
        // Xóa khỏi Marketplace
        string urlDel = $"https://firestore.googleapis.com/v1/projects/gamelord1-49c71/databases/(default)/documents/Marketplace/{marketDocId}";
        using (UnityWebRequest delReq = UnityWebRequest.Delete(urlDel))
        {
            yield return delReq.SendWebRequest();
        }

        // Thêm vào Inventory người mua
        string urlAdd = $"https://firestore.googleapis.com/v1/projects/gamelord1-49c71/databases/(default)/documents/Users/{myWallet}/Inventory";
        string jsonAdd = "{\"fields\": {" +
                         "\"itemId\": {\"stringValue\": \"" + itemData.itemId + "\"}," +
                         "\"isMinted\": {\"booleanValue\": true}," +
                         "\"tokenId\": {\"stringValue\": \"" + tokenId + "\"}" +
                         "}}";

        using (UnityWebRequest addReq = new UnityWebRequest(urlAdd, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonAdd);
            addReq.uploadHandler = new UploadHandlerRaw(bodyRaw);
            addReq.downloadHandler = new DownloadHandlerBuffer();
            addReq.SetRequestHeader("Content-Type", "application/json");
            yield return addReq.SendWebRequest();

            if (addReq.result == UnityWebRequest.Result.Success)
            {
                InventoryManager.Instance?.OpenInventory();
                Destroy(gameObject);
            }
        }
    }

    IEnumerator ReturnItem()
    {
        string urlAdd = $"https://firestore.googleapis.com/v1/projects/gamelord1-49c71/databases/(default)/documents/Users/{myWallet}/Inventory";
        string jsonAdd = "{\"fields\": {" +
                         "\"itemId\": {\"stringValue\": \"" + itemData.itemId + "\"}," +
                         "\"isMinted\": {\"booleanValue\": true}," +
                         "\"tokenId\": {\"stringValue\": \"" + tokenId + "\"}" +
                         "}}";

        using (UnityWebRequest addReq = new UnityWebRequest(urlAdd, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonAdd);
            addReq.uploadHandler = new UploadHandlerRaw(bodyRaw);
            addReq.downloadHandler = new DownloadHandlerBuffer();
            addReq.SetRequestHeader("Content-Type", "application/json");
            yield return addReq.SendWebRequest();

            if (addReq.result == UnityWebRequest.Result.Success)
            {
                string urlDel = $"https://firestore.googleapis.com/v1/projects/gamelord1-49c71/databases/(default)/documents/Marketplace/{marketDocId}";
                using (UnityWebRequest delReq = UnityWebRequest.Delete(urlDel))
                {
                    yield return delReq.SendWebRequest();
                    InventoryManager.Instance?.OpenInventory();
                    Destroy(gameObject);
                }
            }
        }
    }
}