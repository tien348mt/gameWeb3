using UnityEngine;

public class Player_Combat : MonoBehaviour
{
    public Animator animator;
    [SerializeField] private float cooldownAttack = 2f;
    public PlayerDamage playerDamage;
    public GameObject slash;
    private float timer;
    private void Update()
    {
        if(timer > 0)
        {
            timer -= Time.deltaTime;
        }
        if (Input.GetMouseButtonDown(1) && timer <= 0)
        {
            Attack();
            AudioManager.Instance.PlaySFX(0);
        }
    }
    public void Attack()
    {
            animator.SetBool("isAttacking", true);
            timer = cooldownAttack;
        
    }
    public void finishAttack()
    {
        animator.SetBool("isAttacking", false);
    }

    public void EnableSlash()
    {
        slash.SetActive(true);
        playerDamage.DoDamage();
    }

    public void DisableSlash()
    {
        slash.SetActive(false);
    }
}
