using UnityEngine;

public class Player_Combat : MonoBehaviour
{
    public Animator animator;
    [SerializeField] private float cooldownAttack = 2f;
    private float timer;
    private void Update()
    {
        if(timer > 0)
        {
            timer -= Time.deltaTime;
        }
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }
    public void Attack()
    {
        if(timer <=0)
        {
            animator.SetBool("isAttacking", true);
            timer = cooldownAttack;
        }
        
    }
    public void finishAttack()
    {
        animator.SetBool("isAttacking", false);
    }
}
