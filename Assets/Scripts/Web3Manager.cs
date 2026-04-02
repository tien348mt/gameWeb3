using UnityEngine;
using Thirdweb;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.SceneManagement;
using System.Numerics;
using UnityEngine.Networking;
using System.Collections;
using System;
using NUnit.Framework.Interfaces;

public class Web3Manager : MonoBehaviour
{
    public static Web3Manager Instance;
    public TextMeshProUGUI error;
    public TextMeshProUGUI walletText;

    private string contractAddress = "0xf8c7f0840208e12d69b5A4f5B467Fd1B2B9Ca2DC";

    private string contractABI = @"[
    {""inputs"":[],""name"":""nextTokenId"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},
    {""inputs"":[{""internalType"":""address"",""name"":""to"",""type"":""address""},{""internalType"":""string"",""name"":""uri"",""type"":""string""}],""name"":""mintItem"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""nonpayable"",""type"":""function""},
    {""inputs"":[{""internalType"":""uint256"",""name"":""tokenId"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""price"",""type"":""uint256""}],""name"":""listItem"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},
    {""inputs"":[{""internalType"":""uint256"",""name"":""tokenId"",""type"":""uint256""}],""name"":""buyItem"",""outputs"":[],""stateMutability"":""payable"",""type"":""function""},
    {""inputs"":[{""internalType"":""address"",""name"":""owner"",""type"":""address""},{""internalType"":""address"",""name"":""operator"",""type"":""address""}],""name"":""isApprovedForAll"",""outputs"":[{""internalType"":""bool"",""name"":"""",""type"":""bool""}],""stateMutability"":""view"",""type"":""function""}
]";


    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    private void ShowError(string msg)
    {
        if (error != null) error.text = msg;
        Debug.LogError(msg);
    }

    // ================= MINT =================
    public async Task<string> MintNFT(string walletAddress, string itemID, ItemData item, string docId)
    {
        ShowError("Đang mint NFT...");
        try
        {
            var contract = ThirdwebManager.Instance.SDK.GetContract(contractAddress, contractABI);

            string uri = item.metadataUri;
            //"ipfs://bafkreidaaasgjwzoa23wfshm3pjh6tz5hl2imctm334arxgel5sz4twodm";
            //"ipfs://bafkreie7vr2h2fypya4czvyfdxkkxzfv4tyiqbafakxxiyycme6dnewgge";

            await contract.Write("mintItem", walletAddress, uri);

            var nextId = await contract.Read<BigInteger>("nextTokenId");

            BigInteger mintedId = nextId - 1;

            string tokenId = mintedId.ToString();

            ShowError("Mint thành công!" + uri);
            
            return tokenId;
        }
        catch (System.Exception e)
        {
            if (e.Message.Contains("eth_getTransactionReceipt"))
            {
                ShowError("Mint pending nhưng vẫn thành công (WebGL bug)");
                return "1";
            }

            ShowError("Mint lỗi: " + e.Message);
            return "failed";
        }
    }

    // ================= LIST =================
    public async Task<string> ListNFT(string tokenId, string priceEth)
    {
        try
        {
            var contract = ThirdwebManager.Instance.SDK.GetContract(contractAddress, contractABI);
            string priceWeiString = Utils.ToWei(priceEth);
            BigInteger priceWei = BigInteger.Parse(priceWeiString);
            string myAddress = await ThirdwebManager.Instance.SDK.wallet.GetAddress();

            bool approved = await contract.Read<bool>("isApprovedForAll", myAddress, contractAddress);
            if (!approved)
            {
                await contract.Write("setApprovalForAll", contractAddress, true);
            }

            await contract.Write("listItem", tokenId, priceWei);
            return "success";
        }
        catch (System.Exception e)
        {
            ShowError("List lỗi: " + e.Message);
            return "failed";
        }
    }
    // ================= BUY =================
    public async Task<string> BuyNFT(string tokenId, string priceEth)
    {
        ShowError("Đang xử lý giao dịch...");
        try
        {
            var contract = ThirdwebManager.Instance.SDK.GetContract(contractAddress, contractABI);

            Debug.Log("--- DEBUG BUY ---");
            Debug.Log("Token ID: " + tokenId);
            Debug.Log("Giá nhận từ UI (Eth): " + priceEth);

            string valueWei = Utils.ToWei(priceEth);
            Debug.Log("Giá trị gửi lên Blockchain (Wei): " + valueWei);

            string currentWallet = await ThirdwebManager.Instance.SDK.wallet.GetAddress();
            Debug.Log("Địa chỉ ví đang gọi lệnh mua: " + currentWallet);

            await contract.Write(
                "buyItem",
                new TransactionRequest() { value = valueWei },
                tokenId
            );

            ShowError("Mua thành công!");
            return "success";
        }
        catch (System.Exception e)
        {
            if (e.Message.Contains("eth_getTransactionReceipt")) return "success";

            Debug.LogError("Lỗi chi tiết: " + e.ToString());
            ShowError("Buy lỗi: " + e.Message);
            return "failed";
        }
    }

    public void Logout()
    {
        ThirdwebManager.Instance.SDK.wallet.Disconnect();
        if (walletText != null) walletText.text = "Connect Wallet";
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("Login");
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Login" || scene.name == "GamePlay")
        {
            GameObject walletObj = GameObject.Find("WalletTextName");
            if (walletObj != null) walletText = walletObj.GetComponent<TextMeshProUGUI>();
            GameObject errorObj = GameObject.Find("Loi");
            if (errorObj != null) error = errorObj.GetComponent<TextMeshProUGUI>();
        }
    }

    public IEnumerator PostToMarketplace(string tokenId, string sellerWallet, string itemId, ItemData item)
    {
        string urlMarket = "https://firestore.googleapis.com/v1/projects/gamelord1-49c71/databases/(default)/documents/Marketplace";
        string jsonData = "{\"fields\": {" +
                          "\"tokenId\": {\"stringValue\": \"" + tokenId + "\"}," +
                          "\"itemId\": {\"stringValue\": \"" + itemId + "\"}," +
                          "\"price\": {\"stringValue\": \"" + item.basePrice + "\"}," +
                          "\"armor\": {\"stringValue\": \"" + item.armor + "\"}," +
                          "\"attack\": {\"stringValue\": \"" + item.attack + "\"}," +
                          "\"seller\": {\"stringValue\": \"" + sellerWallet + "\"}" +
                          "}}";

        using (UnityWebRequest request = new UnityWebRequest(urlMarket, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
        }
    }

    public IEnumerator UpdateFirebaseMintStatus(string tokenId, string docId, string wallet)
    {
        string url = $"https://firestore.googleapis.com/v1/projects/gamelord1-49c71/databases/(default)/documents/Users/{wallet}/Inventory/{docId}?updateMask.fieldPaths=isMinted&updateMask.fieldPaths=tokenId";
        string json = "{\"fields\": {" +
                      "\"isMinted\": {\"booleanValue\": true}," +
                      "\"tokenId\": {\"stringValue\": \"" + tokenId + "\"}" +
                      "}}";

        using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
        }
    }
    public IEnumerator DeleteItemFromFirebase(string wallet, string docId, GameObject uiSlot)
    {
        string url = $"https://firestore.googleapis.com/v1/projects/gamelord1-49c71/databases/(default)/documents/Users/{wallet}/Inventory/{docId}";

        using (UnityWebRequest request = new UnityWebRequest(url, "DELETE"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log(">>> Đã xóa item khỏi Firebase thành công!" + wallet +" aaa " + docId);
                if (uiSlot != null)
                {
                    Destroy(uiSlot);
                }
            }
            else
            {
                Debug.LogError(">>> Lỗi xóa Firebase: " + request.error);
            }
        }
    }
}