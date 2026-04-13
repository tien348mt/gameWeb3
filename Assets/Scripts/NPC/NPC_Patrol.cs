using System.Collections;
using UnityEngine;

public class NPC_Patrol : MonoBehaviour
{
    public Vector2[] patrolPoints;
    public Vector2 target;

    public float speed = 2f;
    public float pauseDuration = 1.5f;

    private bool isPaused;
    private int currentPatrolIndex;
    private Rigidbody2D rb;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        StartCoroutine(SetPatrolPoint());
    }
    void FixedUpdate()
    {
        if (isPaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Move();

        if (Vector2.Distance(transform.position, target) < 0.1f)
        {
               StartCoroutine(SetPatrolPoint());
        }
    }

    private void Move()
    {
       
        Vector2 currentPos = transform.position;
        Vector2 direction = (target - currentPos).normalized;

        rb.linearVelocity = direction * speed;

        if (direction.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (direction.x < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    IEnumerator SetPatrolPoint()
    {
        isPaused = true;
        animator.Play("idle");
        yield return new WaitForSeconds(pauseDuration);

        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        target = patrolPoints[currentPatrolIndex];
        animator.Play("walk");
        isPaused = false;
       
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isPaused = false;
    }
}