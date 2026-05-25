using UnityEngine;
using DG.Tweening;
using TMPro;

public class RewardUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    public TextMeshProUGUI coin;
    public TextMeshProUGUI exp;

    private Tween currentTween;

    private void Start()
    {
        panel.SetActive(false);
    }

    public void ShowReward()
    {
        currentTween?.Kill();

        panel.SetActive(true);

        panel.transform.localScale = Vector3.zero;

        currentTween = panel.transform
            .DOScale(Vector3.one, 1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                DOVirtual.DelayedCall(1f, () =>
                {
                    panel.transform
                        .DOScale(Vector3.zero, 0.3f)
                        .SetEase(Ease.InBack)
                        .OnComplete(() =>
                        {
                            panel.SetActive(false);
                        });
                });
            });
    }
}