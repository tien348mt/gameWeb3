using UnityEngine;

public class NPC_Talk : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator anim;
    public Animator interaction;

    private void OnEnable()
    {
        rb.linearVelocity = Vector3.zero;
        anim.Play("idle");
        interaction.Play("open");
    }
    private void OnDisable()
    {
        interaction.Play("close");
    }
}
