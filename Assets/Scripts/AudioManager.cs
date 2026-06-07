using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip[] musicClips;

    [Header("SFX")]
    [SerializeField] private AudioClip[] sfxClips;

    private const string KeyMusic = "vol_music";
    private const string KeySFX = "vol_sfx";

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load volume đã lưu (mặc định 1 nếu chưa có)
        musicSource.volume = PlayerPrefs.GetFloat(KeyMusic, 1f);
        sfxSource.volume = PlayerPrefs.GetFloat(KeySFX, 1f);
    }

    // ── Music ────────────────────────────────────────────────

    public void PlayMusic(int index, bool loop = true)
    {
        if (!IsValid(musicClips, index)) return;
        var clip = musicClips[index];
        if (musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic() => musicSource.Stop();

    // ── SFX ──────────────────────────────────────────────────

    public void PlaySFX(int index)
    {
        if (!IsValid(sfxClips, index)) return;
        sfxSource.PlayOneShot(sfxClips[index]);
    }

    // ── Volume (gắn vào Slider.onValueChanged) ───────────────

    public void SetMusicVolume(float v)
    {
        musicSource.volume = v;
        PlayerPrefs.SetFloat(KeyMusic, v);
    }

    public void SetSFXVolume(float v)
    {
        sfxSource.volume = v;
        PlayerPrefs.SetFloat(KeySFX, v);
    }

    public float GetMusicVolume() => musicSource.volume;
    public float GetSFXVolume() => sfxSource.volume;

    // ── Helper ───────────────────────────────────────────────

    bool IsValid(AudioClip[] arr, int i)
    {
        if (arr == null || i < 0 || i >= arr.Length || arr[i] == null)
        {
            Debug.LogWarning($"[AudioManager] Index {i} không hợp lệ.");
            return false;
        }
        return true;
    }
}