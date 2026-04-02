using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image iconImage;
    public Image frameImage;
    public Button actionButton;
    public Text buttonText;
    public TextMeshProUGUI walletText;
    public TextMeshProUGUI error;

    [Header("Item Data")]
    private ItemData currentItem;
    private string documentId;
    private bool isMinted = false;

    public void Setup(ItemData data, TextMeshProUGUI wallet, string docId, bool mintedStatus = false)
    {
        currentItem = data;
        walletText = wallet;
        documentId = docId;
        isMinted = mintedStatus;
     
        if (currentItem != null)
        {
            iconImage.sprite = currentItem.itemIcon;
            iconImage.enabled = true;
        }

        if (isMinted)
        {
            SetAsNFTStyle();
        }
        else
        {
            buttonText.text = "MINT NFT";
            actionButton.interactable = true;
        }

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(OnMintButtonClick);
    }

    public void SetAsNFTStyle()
    {
        isMinted = true;
        buttonText.text = "ON MARKET";
        actionButton.interactable = false;

        if (frameImage != null)
        {
            frameImage.color = new Color(1f, 0.84f, 0f);
        }
    }

    public async void OnMintButtonClick()
    {
        string myWallet = walletText.text.Trim();
        if (string.IsNullOrEmpty(myWallet) || myWallet.Length < 10) return;

        actionButton.interactable = false;
        buttonText.text = "MINTING...";

        try
        {
           
            string tokenId = await Web3Manager.Instance.MintNFT(myWallet, currentItem.itemId, currentItem, documentId);

            if (tokenId != "failed")
            {
                Web3Manager.Instance.StartCoroutine(Web3Manager.Instance.PostToMarketplace(tokenId, myWallet, currentItem.itemId, currentItem));
                //Web3Manager.Instance.StartCoroutine(Web3Manager.Instance.UpdateFirebaseMintStatus(tokenId, documentId, myWallet));
                Web3Manager.Instance.StartCoroutine(Web3Manager.Instance.DeleteItemFromFirebase(myWallet, documentId,this.gameObject));
                SetAsNFTStyle();
                if (error != null) error.text = "Đang xử lý niêm yết ngầm...";
                _ = Web3Manager.Instance.ListNFT(tokenId, currentItem.basePrice);
            }
            else
            {
                ResetButton();
            }
        }
        catch (System.Exception e)
        {
            if (error != null) error.text = "Lỗi: " + e.Message;
            ResetButton();
        }
    }

    private void ResetButton()
    {
        if (this != null && !isMinted)
        {
            actionButton.interactable = true;
            buttonText.text = "MINT NFT";
        }
    }
}