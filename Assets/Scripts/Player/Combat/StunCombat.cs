using System.Collections;
using UnityEngine;

public class StunCombat : MonoBehaviour, ICombat
{
    private bool isProtected = false;
    private float lightStunDuration = 2f;
    private float heavyStunDuration = 4f;
    private ParticleSystem parrySparks;
    private ParticleSystem chargeSparks;
    private IMovement playerMovement;
    private bool isAttackingHeavy = false;
    private bool isChargingAttack = false;

    void Awake()
    {
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem particle in particles) {
            if (particle.gameObject.CompareTag("ParrySparkParticles"))
                parrySparks = particle;
            else if (particle.gameObject.CompareTag("ChargeAttackParticles"))
                chargeSparks = particle;
        }
        playerMovement = GetComponent<IMovement>();
    }

    #region ICombat implementation

    public void ChargeAttack()
    {
        isChargingAttack = true;
        chargeSparks.Play();
        playerMovement.DisableMovement(true);
    }

    public void ExecuteAttack(bool isHeavyAttack)
    {
        InterruptCharge();

        if (isHeavyAttack)
        {
            StartCoroutine(HeavyAttack());
        }
        else
        {
            Collider[] collisions = Physics.OverlapSphere(transform.position, 0.5f);

            foreach (Collider collider in collisions)
            {
                StunCombat enemy = collider.gameObject.GetComponentInParent<StunCombat>();
                if (enemy != null && enemy != this)
                {
                    Debug.Log("Stun attack is hitting!");
                    enemy.ReceiveAttack(lightStunDuration);
                }
            }
        }

            Debug.Log("Stun attack executed!");
        
    }

    IEnumerator HeavyAttack()
    {
        //dash
        playerMovement.ExecuteAbility(MovementAbilityType.Dash);
        isProtected = true;
        isAttackingHeavy = true;

        // after dash player is no longer attacking
        yield return new WaitForSeconds(0.2f);
        isAttackingHeavy = false;
        isProtected = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isAttackingHeavy) return;

        StunCombat enemy = collision.gameObject.GetComponentInParent<StunCombat>();
        if (enemy != null && enemy != this)
        {
            Debug.Log("Stun attack is hitting!");
            enemy.ReceiveAttack(heavyStunDuration, false);
        }
    }

    private void InterruptCharge()
    {
        isChargingAttack = false;
        chargeSparks.Stop(); 
        playerMovement.DisableMovement(false);
    }

    private void LightAttack()
    {

    }

    public void ReceiveAttack(float? attackEffect = null, bool unableToParry = false)
    {
        if (unableToParry || !isProtected)
        {
            Debug.Log("Player got hit and is unprotected");

            if (isChargingAttack)
                InterruptCharge();

            StartCoroutine(playerMovement.DisableMovement(attackEffect.Value));
            // feedback visual
        }
        else
        {
            Debug.Log("Player got hit but is protected");
            parrySparks.Play();
        }
    }

    public IEnumerator ParryAttack()
    {
        isProtected = true; 
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Parry over! Player unprotected.");
        isProtected = false;

    }
    #endregion


}
