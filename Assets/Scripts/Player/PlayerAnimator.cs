using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private float motion = 0.0f;
    public float Motion { set => motion = value; }

    private bool isChargingAttack = false;
    private bool isAttacking = false;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        animator.SetFloat("MotionSpeed", motion, 0.1f, Time.deltaTime);
        
        if (isAttacking)
        {
            animator.SetFloat("HeavyMelee", 1f, 0.1f, Time.deltaTime);
        }
        else if (isChargingAttack)
        {
            animator.SetFloat("HeavyMelee", 0.5f, 0.1f, Time.deltaTime);
        }
        else
        {
            animator.SetFloat("HeavyMelee", 0f, 0.1f, Time.deltaTime);
        }
    }

    public void TriggerHeavyAttack()
    {
        isAttacking = true;
    }

    public void TriggerLightAttack()
    {
        animator.SetTrigger("LightMelee");
    }

    public void TriggerChargeAttack()
    {
        animator.Play("Motion_HeavyMelee", 0, 0f); // to restart animation clips
        isChargingAttack = true;
    }

    public void TriggerStun()
    {
        animator.SetTrigger("Stun");
    }

    public void TriggerAttackStun()
    {
        animator.SetTrigger("AttackStun");
    }

    public void TriggerParrySuccess()
    {
        animator.SetTrigger("ParrySuccess");
    }

    public void TriggerParry()
    {
        animator.SetTrigger("Parry");
    }

    public void CancelChargeAttack()
    {
        isChargingAttack = false;
    }

    public void CancelAttack()
    {
        isAttacking = false;
    }
}