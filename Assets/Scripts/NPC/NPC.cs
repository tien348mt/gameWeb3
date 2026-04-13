using UnityEngine;

public class NPC : MonoBehaviour
{
    public CapsuleCollider2D col;
    public Rigidbody2D rb;
    public NPC_Patrol patrol;
    public GameObject interaction;

    public Animator animator;
    void Start()
    {
        if (patrol != null) patrol.enabled = true;
        if (interaction != null) interaction.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (patrol != null) patrol.enabled = false;
            if (interaction != null) interaction.SetActive(true);
            rb.bodyType = RigidbodyType2D.Static;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            if (patrol != null) patrol.enabled = true;
            if (interaction != null) interaction.SetActive(false);
            animator.Play("walk");
        }
    }
}