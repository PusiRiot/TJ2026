using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunCombat : MonoBehaviour, ICombat
{

    private IMovement playerMovement;

    private AbstractLight playerLight;

    private ParticleSystem parrySparks;
    private ParticleSystem chargeSparks;
    private ParticleSystem attackSparks;

    private List<Material> playerBlinkMaterials = new();

    private float lightStunDuration = 2f;
    private float heavyStunDuration = 4f;
    private float lightOffDuration = 1;
    private float timeoutDuration = 1;

    private bool isProtectedByParry = false;
    private bool isAttackingHeavy = false;
    private bool isChargingAttack = false;
    private bool isProtectedByTimeout = false;

    private float currentBlinkAmount = 0.0f;
    private float timeoutStep;

    #region MonoBehaviour
    void Awake()
    {
        playerMovement = GetComponent<IMovement>();

        // Initialize player light
        playerLight = GetComponentInChildren<AbstractLight>();

        // Particles initialization
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem particle in particles) {
            if (particle.gameObject.CompareTag("ParrySparkParticles"))
                parrySparks = particle;
            else if (particle.gameObject.CompareTag("ChargeAttackParticles"))
                chargeSparks = particle;
            else if (particle.gameObject.CompareTag("AttackParticles"))
                attackSparks = particle;
        }

        // Hurt blink materials initialization
        currentBlinkAmount = 0f;
        MeshRenderer[] playerMeshes = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer playerMesh in playerMeshes)
        {
            foreach(Material mat in playerMesh.materials)
            {
                if (mat.HasFloat("_BlinkAmount"))
                {
                    playerBlinkMaterials.Add(mat);
                    mat.SetFloat("_BlinkAmount", currentBlinkAmount);
                }
            }
        }

        lightStunDuration = GameManager.Instance.GetLightStunDuration();
        heavyStunDuration = GameManager.Instance.GetHeavyStunDuration();
    }

    void Update()
    {
        if (isProtectedByTimeout)
        {
            TimeOut();
        }
    }
    #endregion

    #region Attack methods
    public void ExecuteAttack(bool isHeavyAttack)
    {
        attackSparks.Play();

        if (isHeavyAttack)
        {
            InterruptCharge();
            StartCoroutine(HeavyAttack());
        }
        else
        {
            LightAttack();
        }
    }

    IEnumerator HeavyAttack()
    {
        //dash
        StartCoroutine(playerMovement.Dash(true));
        isProtectedByParry = true;
        isAttackingHeavy = true;

        // after dash player is no longer attacking
        yield return new WaitForSeconds(GameManager.Instance.GetDashDuration());
        isAttackingHeavy = false;
        isProtectedByParry = false;
    }

    void LightAttack()
    {
        Collider[] collisions = Physics.OverlapSphere(transform.position, 1f);

        foreach (Collider collider in collisions)
        {
            StunCombat enemy = collider.gameObject.GetComponentInParent<StunCombat>();
            if (enemy != null && enemy != this)
            {
                enemy.ReceiveAttack(lightStunDuration);
            }
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (!isAttackingHeavy) return;

        StunCombat enemy = collision.gameObject.GetComponentInParent<StunCombat>();
        if (enemy != null && enemy != this)
        {
            enemy.ReceiveAttack(heavyStunDuration, false);
        }
    }
    #endregion

    #region Charge methods
    public void ChargeAttack()
    {
        if (isChargingAttack || isAttackingHeavy) return;

        isChargingAttack = true;
        chargeSparks.Play();
        playerMovement.DisableMovement(true);
    }

    public void InterruptCharge()
    {
        isChargingAttack = false;
        chargeSparks.Stop();
        playerMovement.DisableMovement(false);
    }
    #endregion

    #region Hurt and parry methods
    public IEnumerator ParryAttack()
    {
        isProtectedByParry = true;
        yield return new WaitForSeconds(1f);
        isProtectedByParry = false;

    }

    public void ReceiveAttack(float attackDuration, bool unableToParry = false)
    {
        if (isProtectedByTimeout) return; // can't be attacked while cooling down another attack

        if ((!isProtectedByParry || unableToParry))
        {
            // turn off light
            lightOffDuration = attackDuration;
            playerLight.TurnOff();

            // enable protection so that player can recover
            timeoutDuration = attackDuration + 1.0f;
            isProtectedByTimeout = true;

            // interrupt player attacks
            if (isChargingAttack)
                InterruptCharge();  // TODO: this doesnt do anything on the actual attack

            // stun effect
            StartCoroutine(playerMovement.DisableMovement(attackDuration));
        }
        else
        {
            parrySparks.Play();
        }
    }

    void TimeOut()
    {
        timeoutStep += Time.deltaTime;

        float normalizedInvencibilityStep = timeoutStep / timeoutDuration;
        ChangeHurtGlow(normalizedInvencibilityStep);

        if (timeoutStep >= lightOffDuration)
            playerLight.TurnOn();

        if (timeoutStep >= timeoutDuration)
        {
            currentBlinkAmount = 0f;
            foreach (Material mat in playerBlinkMaterials)
                mat.SetFloat("_BlinkAmount", currentBlinkAmount);

            timeoutStep = 0;
            isProtectedByTimeout = false;
        }
    }

    void ChangeHurtGlow(float step)
    {
        if (step < 0.5f)
        {
            float renormalizedStep = step / 0.5f;
            currentBlinkAmount = Math.Clamp(renormalizedStep, 0f, 1f);
        }
        else
        {
            step = step - 0.5f;
            float renormalizedStep = step / 0.5f;
            currentBlinkAmount = Math.Clamp(renormalizedStep, 0f, 1f);
        }

        foreach (Material mat in playerBlinkMaterials)
            mat.SetFloat("_BlinkAmount", currentBlinkAmount);
    }
    #endregion
}
