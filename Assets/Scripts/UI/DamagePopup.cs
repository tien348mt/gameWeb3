using TMPro;
using UnityEngine;
using DG.Tweening;

public class DamagePopup : MonoBehaviour
{
    public TextMeshPro textMesh;

    public void Setup(int damageAmount, Vector3 position)
    {
        transform.position = position;
        textMesh.text = "-" + damageAmount.ToString();
        textMesh.color = new Color(1, 0.2f, 0.2f, 1); // màu đỏ

        // Animation bay lên trong 1.5 giây + fade
        transform.DOLocalMoveY(transform.localPosition.y + 1.5f, 1.5f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                Destroy(gameObject);   // Destroy trong OnComplete
            });

        textMesh.DOFade(0, 1.5f).SetEase(Ease.InQuad);
    }
}