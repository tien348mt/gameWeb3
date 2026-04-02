using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Animator animator;

    private Rigidbody2D rb;
    private Vector2 movement;
    private int FacingDirection = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        if(movement.x < 0 && transform.localScale.x > 0 
            || movement.x > 0 && transform.localScale.x < 0)
        {
            Flip();
        }
        animator.SetFloat("horizontal", Mathf.Abs(movement.x));
        animator.SetFloat("vertical", Mathf.Abs(movement.y));
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
    void Flip()
    {
        FacingDirection *= -1;
        transform.localScale = new Vector3(transform.localScale.x * FacingDirection, transform.localScale.y, transform.localScale.z);
    }
}