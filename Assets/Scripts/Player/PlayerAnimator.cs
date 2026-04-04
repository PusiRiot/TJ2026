using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    #region Variables
    private Animator animator;
    private float motion = 0.0f;
    public float Motion { set => motion = value; }

    private bool isChargingAttack = false;
    private bool isAttacking = false;
    private PlayerAnimationsSet animationSet; // assign in inspector
    #endregion

    #region MonoBehaviour

    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        var overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);

        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        overrideController.GetOverrides(overrides);

        for (int i = 0; i < overrides.Count; i++)
        {
            var original = overrides[i].Key;

            // Match by name
            if (original.name.Contains("Idle"))
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(original, animationSet.Idle);

            if (original.name.Contains("Walk"))
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(original, animationSet.Walk);

            if (original.name.Contains("Melee"))
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(original, animationSet.Melee);

            if (original.name.Contains("Charge"))
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(original, animationSet.Charge);

            if (original.name.Contains("Stun"))
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(original, animationSet.Stun);

            if (original.name.Contains("StunAttack"))
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(original, animationSet.StunAttack);

            if (original.name.Contains("Parry"))
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(original, animationSet.Parry);

            if (original.name.Contains("ParrySuccess"))
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(original, animationSet.ParrySuccess);
        }       

        overrideController.ApplyOverrides(overrides);
        animator.runtimeAnimatorController = overrideController;
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
    #endregion

    public void Initialize(PlayerAnimationsSet animationSet)
    {
        this.animationSet = animationSet;
    }    

    public void TriggerHeavyAttack()
    {
        isAttacking = true;
    }

    public void TriggerLightAttack()
    {
        animator.SetTrigger("LightMelee");
    }

    public void CancelAttack()
    {
        isAttacking = false;
    }

    public void TriggerChargeAttack()
    {
        animator.Play("Motion_HeavyMelee", 0, 0f); // to restart animation clips
        isChargingAttack = true;
    }

    public void CancelChargeAttack()
    {
        isChargingAttack = false;
    }

    public void TriggerStun()
    {
        animator.SetTrigger("Stun");
    }

    public void CancelStun()
    {
        animator.SetTrigger("StunStop");
    }

    public void TriggerStunAttack()
    {
        animator.SetTrigger("StunAttack");
    }

    public void TriggerParrySuccess()
    {
        animator.SetTrigger("ParrySuccess");
    }

    public void TriggerParry()
    {
        animator.SetTrigger("Parry");
    }

    public void CancelParry()
    {
        animator.SetTrigger("ParryStop");
    }

    public void TriggerLightDamage()
    {
        animator.SetTrigger("LightDamage");
    }
}