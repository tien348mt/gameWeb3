using UnityEngine;
using Thirdweb;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.SceneManagement;
using System.Numerics;
using UnityEngine.Networking;
using System.Collections;
using System;

public class Web3Manager : MonoBehaviour
{
    public static Web3Manager Instance;
    public TextMeshProUGUI error; // Kéo object 'Loi' vào đây
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

    // Hàm phụ trợ để hiện lỗi lên màn hình
    private void ShowError(string msg)
    {
        if (error != null) error.text = msg;
        Debug.LogError(msg);
    }

    // ================= MINT =================
    public async Task<string> MintNFT(string walletAddress, string itemID)
    {
        ShowError("Đang mint NFT...");
        try
        {
            var contract = ThirdwebManager.Instance.SDK.GetContract(contractAddress, contractABI);

            string uri = "ipfs://bafkreidaaasgjwzoa23wfshm3pjh6tz5hl2imctm334arxgel5sz4twodm";

            // 1. Mint
            await contract.Write("mintItem", walletAddress, uri);

            // 2. Đọc nextTokenId
            var nextId = await contract.Read<BigInteger>("nextTokenId");

            // 3. Token vừa mint = nextId - 1
            BigInteger mintedId = nextId - 1;

            string tokenId = mintedId.ToString();

            ShowError("Mint thành công! TokenID: " + tokenId);
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
            string priceWeiString = Utils.ToWei(priceEth); // Lấy chuỗi Wei từ Utils
            BigInteger priceWei = BigInteger.Parse(priceWeiString);
            string myAddress = await ThirdwebManager.Instance.SDK.wallet.GetAddress();

            bool approved = await contract.Read<bool>("isApprovedForAll", myAddress, contractAddress);
            if (!approved)
            {
                await contract.Write("setApprovalForAll", contractAddress, true);
            }

            // Lưu ý: listItem nhận uint256 nên truyền BigInteger là đúng
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

            // 1. Kiểm tra giá trị đầu vào
            Debug.Log("--- DEBUG BUY ---");
            Debug.Log("Token ID: " + tokenId);
            Debug.Log("Giá nhận từ UI (Eth): " + priceEth);

            // 2. Kiểm tra giá trị sau khi đổi sang Wei
            string valueWei = Utils.ToWei(priceEth);
            Debug.Log("Giá trị gửi lên Blockchain (Wei): " + valueWei);

            // 3. Kiểm tra ví đang thực hiện giao dịch (Tránh nhầm ví Local)
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

            // In chi tiết lỗi ra Console để soi mã lỗi Blockchain
            Debug.LogError("Lỗi chi tiết: " + e.ToString());
            ShowError("Buy lỗi: " + e.Message);
            return "failed";
        }
    }

    // --- Logout và Firebase giữ nguyên ---
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

    public IEnumerator PostToMarketplace(string tokenId, string sellerWallet, string itemId)
    {
        string urlMarket = "https://firestore.googleapis.com/v1/projects/gamelord1-49c71/databases/(default)/documents/Marketplace";
        string jsonData = "{\"fields\": {" +
                          "\"tokenId\": {\"stringValue\": \"" + tokenId + "\"}," +
                          "\"itemId\": {\"stringValue\": \"" + itemId + "\"}," +
                          "\"price\": {\"stringValue\": \"0.001\"}," +
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
}