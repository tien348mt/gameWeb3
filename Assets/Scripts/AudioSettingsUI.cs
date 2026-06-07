using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    void Start()
    {
        // Set giá trị slider theo volume đã lưu
        musicSlider.value = AudioManager.Instance.GetMusicVolume();
        sfxSlider.value = AudioManager.Instance.GetSFXVolume();

        // Gắn sự kiện
        musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
    }

    void OnDestroy()
    {
        musicSlider.onValueChanged.RemoveListener(AudioManager.Instance.SetMusicVolume);
        sfxSlider.onValueChanged.RemoveListener(AudioManager.Instance.SetSFXVolume);
    }
}