using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LoadingScene : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup canvasGroup;
    public Image loadingImage;

    [Header("Settings")]
    public float fadeInDuration = 0.5f;
    public float holdDuration = 2f;
    public float fadeOutDuration = 0.5f;

    void Awake()
    {
        // Ẩn ngay từ đầu
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
    private void Start()
    {
        StartLoading();
    }

    public void StartLoading()
    {
        canvasGroup.blocksRaycasts = true;

        DOTween.Sequence()
            .Append(canvasGroup.DOFade(1f, fadeInDuration))
            .AppendInterval(holdDuration)
            .Append(canvasGroup.DOFade(0f, fadeOutDuration))
            .OnComplete(() =>
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                AudioManager.Instance.PlayMusic(0);
            });
    }
}