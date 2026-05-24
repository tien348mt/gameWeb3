using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [Header("UI References")]
    public Image portrait;
    public TextMeshProUGUI actorName;
    public TextMeshProUGUI dialogueText;
    public CanvasGroup canvasGroup;
    public bool isDialogueActive;
    public bool isWaitingForAccept = false;
    public DialogueSO currentDialogue;
    private int dialogueIndex;

    public UnityEvent onDialogueEnded = new UnityEvent();
    public UnityEvent onLastLineReached = new UnityEvent();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void StartDialogue(DialogueSO dialogueSO, UnityAction onEndAction = null)
    {
        currentDialogue = dialogueSO;
        dialogueIndex = 0;
        isDialogueActive = true;
        onDialogueEnded.RemoveAllListeners();
        if (onEndAction != null) onDialogueEnded.AddListener(onEndAction);

        ShowDialogue();
    }

    public void AdvanceDialogue()
    {
        if (isWaitingForAccept) return;
        dialogueIndex++;
        if (dialogueIndex < currentDialogue.lines.Length)
            ShowDialogue();
        else
            EndDialogue();
    }

    public void ShowDialogue()
    {
        DialogueLine line = currentDialogue.lines[dialogueIndex];
        portrait.sprite = line.speaker.portrait;
        actorName.text = line.speaker.actorName;
        dialogueText.text = line.text;

        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        if (dialogueIndex == currentDialogue.lines.Length - 1)
        {
            onLastLineReached?.Invoke();
        }
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        isWaitingForAccept = false;
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        onDialogueEnded?.Invoke();
        onDialogueEnded.RemoveAllListeners();
    }
}