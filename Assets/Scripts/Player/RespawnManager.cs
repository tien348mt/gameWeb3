using UnityEngine;
using DG.Tweening;

public class RespawnManager : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup blackPanel; // Panel màu đen, alpha = 1

    [Header("Respawn")]
    public Transform[] respawnPoints;
    public Transform player;

    [SerializeField] private float fadeDuration = 1f;

    public void Respawn()
    {
        blackPanel.alpha = 1f;
        blackPanel.gameObject.SetActive(true);
        MovePlayerToNearestRespawn();

        blackPanel.DOFade(0f, fadeDuration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                blackPanel.gameObject.SetActive(false);
                PlayerStats.Instance.currentHp = PlayerStats.Instance.maxHp;
            });
    }

    private void MovePlayerToNearestRespawn()
    {
        if (respawnPoints == null || respawnPoints.Length == 0) return;

        Transform nearest = respawnPoints[0];
        float minDist = Vector3.Distance(player.position, nearest.position);

        foreach (Transform point in respawnPoints)
        {
            float dist = Vector3.Distance(player.position, point.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = point;
            }
        }

        player.position = nearest.position;
    }
}