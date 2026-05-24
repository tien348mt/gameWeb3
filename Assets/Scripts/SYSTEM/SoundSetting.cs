using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class SoundSetting : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public void OpenSoundSetting()
    {
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
    public void CloseSoundSetting()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
