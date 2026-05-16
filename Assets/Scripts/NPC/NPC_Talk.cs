using UnityEngine;

public class NPC_Talk : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator anim;
    public Animator interaction;
    public DialogueSO dialogueSO;
    public NPC_QuestGiver questGiver;

    private void OnEnable()
    {
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

        if (anim != null)
            anim.Play("idle");

        if (interaction != null)
            interaction.Play("open");
    }

    private void OnDisable()
    {
        if (interaction != null)
            interaction.Play("close");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            questGiver.OnPlayerTalk();
        }
    }
}