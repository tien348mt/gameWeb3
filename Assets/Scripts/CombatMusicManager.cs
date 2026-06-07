using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Theo dõi trạng thái combat của tất cả enemy.
/// Nếu có ít nhất 1 enemy đang combat → phát nhạc chiến đấu.
/// Tất cả về idle → phát nhạc bình thường.
/// </summary>
public class CombatMusicManager : MonoBehaviour
{
    public static CombatMusicManager Instance { get; private set; }

    [SerializeField] private int musicIndexNormal = 0;
    [SerializeField] private int musicIndexCombat = 1;

    private readonly HashSet<BaseEnemy> combatEnemies = new();
    private bool isInCombat = false;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>Gọi khi enemy bắt đầu Chase hoặc Attack.</summary>
    public void EnterCombat(BaseEnemy enemy)
    {
        combatEnemies.Add(enemy);
        UpdateMusic();
    }

    /// <summary>Gọi khi enemy về Idle hoặc Return.</summary>
    public void ExitCombat(BaseEnemy enemy)
    {
        combatEnemies.Remove(enemy);
        UpdateMusic();
    }

    void UpdateMusic()
    {
        bool shouldCombat = combatEnemies.Count > 0;
        if (shouldCombat == isInCombat) return;

        isInCombat = shouldCombat;
        AudioManager.Instance.PlayMusic(isInCombat ? musicIndexCombat : musicIndexNormal);
    }
}
