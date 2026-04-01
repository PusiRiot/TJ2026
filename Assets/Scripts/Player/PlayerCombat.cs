using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerCombat : Subject<PlayerCombatEvent>, IObserver<PlayerCombatEvent>
{
    #region Variables
    // Read-only variables are preceded by _ (can't be set to readonly because they have to be initialized on runtime)
    private int _teamIndex = -1;

    // Lives
    private int _maxLives = -1;
    private int currentLives = -1;

    // Combat
    private int _lightMeleeDamage = -1;
    private int _heavyMeleeDamage = -1;

    private float _heavyMeleeDashDuration = -1f;
    private float _heavyMeleeDashSpeedIncrement = -1f;

    private float _heavyMeleeLightOffDuration = -1;
    private float _succesfulParryLightOffDuration = -1;

    private float _deathDuration = -1f;

    private float _parryDuration = -1f;

    // Needed player references
    private PlayerMovement playerMovement;
    private Player player;
    private AbstractLight playerLight;
    private PlayerAnimator playerAnimator;

    // Colliders
    private GameObject regularCollider;
    private GameObject heavyMeleeCollider;
    private GameObject parryCollider;

    // Visual effects
    private ParticleSystem parrySparks;
    private ParticleSystem parryingSparks;
    private ParticleSystem chargeSparks;
    private ParticleSystem attackSparks;
    private ParticleSystem healParticles;
    private List<Material> playerBlinkMaterials = new();
    // Hurt glow
    private float currentBlinkAmount = 0.0f;
    private float hurtGlowStep;
    private float hurtGlowDuration;

    // Boolean control
    private bool isProtectedByParry = false;
    private bool isAttackingHeavy = false;
    private bool isChargingAttack = false;
    private bool isHurtGlowActive = false;
    private bool alreadyHit = false; // to prevent hitting multiple times with the heavy melee dash

    public void Initialize(int teamIndex)
    {
        _teamIndex = teamIndex;

        _deathDuration = GameManager.Instance.DeathDuration();

        _maxLives = GameManager.Instance.GetMaxLives();
        currentLives = _maxLives;

        _lightMeleeDamage = GameManager.Instance.LightMeleeDamage();
        _heavyMeleeDamage = GameManager.Instance.HeavyMeleeDamage();

        _heavyMeleeDashDuration = GameManager.Instance.HeavyMeleeDashDuration();
        _heavyMeleeDashSpeedIncrement = GameManager.Instance.HeavyMeleeDashSpeedIncrement();

        _heavyMeleeLightOffDuration = GameManager.Instance.HeavyMeleeLightOffDuration();
        _succesfulParryLightOffDuration = GameManager.Instance.SuccesfulParryLightOffDuration();

        _parryDuration = GameManager.Instance.ParryDuration();
    }

    #endregion

    #region MonoBehaviour
    void Awake()
    {
        base.AddObserversOnScene();

        playerMovement = GetComponent<PlayerMovement>();

        player = GetComponent<Player>();

        playerAnimator = GetComponent<PlayerAnimator>();

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
            else if (particle.gameObject.CompareTag("ParryingParticles"))
                parryingSparks = particle;
            else if (particle.gameObject.CompareTag("HealParticles"))
                healParticles = particle;
        }

        // Colliders initialization
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders)
        {
            if (c.gameObject.CompareTag("RegularCollider"))
                regularCollider = c.gameObject;
            else if (c.gameObject.CompareTag("HeavyMeleeCollider"))
                heavyMeleeCollider = c.gameObject;
            else if (c.gameObject.CompareTag("ParryCollider"))
                parryCollider = c.gameObject;
        }

        heavyMeleeCollider.SetActive(false);
        parryCollider.SetActive(false);

        // Hurt blink materials initialization
        SkinnedMeshRenderer[] playerMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer playerMesh in playerMeshes)
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
    }

    void Update()
    {
        if (isHurtGlowActive)
        {
            HurtGlowTimeStep();
        }
    }
    #endregion

    #region Attack methods
    public void ExecuteAttack(bool isHeavyAttack)
    {
        if (isHeavyAttack)
        {
            attackSparks.Play();

            InterruptCharge();
            StartCoroutine(HeavyAttack());
        }
        else
        {
            StartCoroutine(LightAttack());
        }
    }

    IEnumerator HeavyAttack()
    {
        playerAnimator.TriggerHeavyAttack();

        //dash
        regularCollider.SetActive(false);
        parryCollider.SetActive(false);
        heavyMeleeCollider.SetActive(true);
        isProtectedByParry = true;
        isAttackingHeavy = true;
        alreadyHit = false;
        playerMovement.Dash(_heavyMeleeDashDuration, _heavyMeleeDashSpeedIncrement);

        // after dash player is no longer attacking
        yield return new WaitForSeconds(GameManager.Instance.GetDashDuration());
        isAttackingHeavy = false;
        isProtectedByParry = false;
        playerAnimator.CancelAttack();
        regularCollider.SetActive(true);
        heavyMeleeCollider.SetActive(false);
    }

    IEnumerator LightAttack()
    {
        playerAnimator.TriggerLightAttack();
        StartCoroutine(playerMovement.DisableMovement(0.4f));

        yield return new WaitForSeconds(0.2f); //TODO: to sync with animation, can be changed later when we have the final one
        attackSparks.Play();

        // check collision
        float range = GameManager.Instance.LightMeleeRange();
        Collider[] collisions = Physics.OverlapSphere(transform.position, range);

        foreach (Collider collider in collisions)
        {
            PlayerCombat enemy = collider.gameObject.GetComponentInParent<PlayerCombat>();
            if (enemy != null && enemy != this)
            {
                // effect
                enemy.ReceiveAttack(_lightMeleeDamage, 0f, true);
            }
        }
    }
   
    void OnCollisionStay(Collision collision)
    {
        if (!isAttackingHeavy) return;

        PlayerCombat enemy = collision.gameObject.GetComponentInParent<PlayerCombat>();
        if (enemy != null && enemy != this && !alreadyHit)
        {
            isAttackingHeavy = false; // to avoid multiple collisions on the same attack
            // effect
            bool succesful = enemy.ReceiveAttack(_heavyMeleeDamage, _heavyMeleeLightOffDuration, false);
            if (!succesful) ParryResponse();

            alreadyHit = true;
        }
    }
    #endregion

    #region Charge methods
    public void ChargeAttack()
    {
        if (isChargingAttack || isAttackingHeavy) return;

        playerAnimator.TriggerChargeAttack();
        isChargingAttack = true;
        chargeSparks.Play();
        playerMovement.DisableMovement(true);
    }

    public void InterruptCharge()
    {
        playerAnimator.CancelChargeAttack();
        isChargingAttack = false;
        chargeSparks.Stop();
        playerMovement.DisableMovement(false);
    }
    #endregion

    #region Hurt and parry methods
    public IEnumerator Parry()
    {
        // animation
        playerAnimator.TriggerParry();

        // effect
        parryingSparks.Play();
        isProtectedByParry = true;
        playerMovement.DisableMovement(true);
        regularCollider.SetActive(false);
        heavyMeleeCollider.SetActive(false);
        parryCollider.SetActive(true);

        yield return new WaitForSeconds(_parryDuration);

        StopParry(false);
    }

    void StopParry(bool parrySuccesfull)
    {
        if (parrySuccesfull)
            playerAnimator.TriggerParrySuccess();
        else
            playerAnimator.CancelParry();

        parryingSparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        parryCollider.SetActive(false);
        regularCollider.SetActive(true);
        isProtectedByParry = false;
        playerMovement.DisableMovement(false);
    }

    public bool ReceiveAttack(int damage, float lightOffDuration, bool unableToParry)
    {
        if (isProtectedByParry && !unableToParry)
        {
            parrySparks.Play();
            StopParry(true);
            return false;
        }

        // disable movement shortly for hit stun
        playerMovement.DisableMovement(0.02f);

        // animation
        playerAnimator.TriggerStunAttack();

        // Light switching
        if (lightOffDuration > 0)
        {
            StopCoroutine(nameof(TurnLightOff));
            StartCoroutine(nameof(TurnLightOff), lightOffDuration);
        }

        // Interrupt player attacks
        if (isChargingAttack)
            player.CancelChargeAttack();

        StopCoroutine(LightAttack());


        // Damage
        currentLives -= damage;
        Notify(PlayerCombatEvent.ReceivedDamage, new int[]{_teamIndex, damage});

        if (currentLives <= 0)
        {
            StartCoroutine(nameof(Death));
        }

        isHurtGlowActive = true;

        return true;
    }

    void ParryResponse()
    {
        playerAnimator.TriggerStun();

        // light switching
        StopCoroutine(nameof(TurnLightOff)); // in case another coroutine is up
        StartCoroutine(nameof(TurnLightOff), _succesfulParryLightOffDuration); // only coroutines started by name can be stopped by name

        // stun for one second
        StartCoroutine(playerMovement.DisableMovement(1.0f));
    }

    IEnumerator Death()
    {
        Notify(PlayerCombatEvent.Death, _teamIndex);
        currentLives = _maxLives;

        // disable actions and world interaction
        player.DisableWorldInteraction();

        // light switching
        StopCoroutine(nameof(TurnLightOff)); // in case another coroutine is up

        Debug.Log("stopped coroutine");
        playerLight.TurnOff(); // don't call method to not start twice the same coroutine

        Debug.Log(_deathDuration);
        yield return new WaitForSeconds(_deathDuration);

        // light switching
        playerLight.TurnOn();

        // enable actions and world interaction
        player.EnableWorldInteraction();

        Notify(PlayerCombatEvent.BackToLife, _teamIndex);
    }

    IEnumerator TurnLightOff(float duration)
    {
        playerLight.TurnOff();
        yield return new WaitForSeconds(duration);
        playerLight.TurnOn();

    }

    #region Hurt glow
    void HurtGlowTimeStep()
    {
        hurtGlowStep += Time.deltaTime;

        float normalizedInvencibilityStep = hurtGlowStep / hurtGlowDuration;
        ChangeHurtGlow(normalizedInvencibilityStep);

        if (hurtGlowStep >= hurtGlowDuration)
        {
            currentBlinkAmount = 0f;
            foreach (Material mat in playerBlinkMaterials)
                mat.SetFloat("_BlinkAmount", currentBlinkAmount);

            hurtGlowStep = 0;
            isHurtGlowActive = false;
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

    #endregion

    #region IObserver

    public void OnNotify(PlayerCombatEvent evt, object data = null)
    {
        switch (evt)
        {
            case PlayerCombatEvent.ReceivedHeal:
                {
                    int[] dataHeal = data as int[];
                    int teamIndex = dataHeal[0];
                    int healAmount = dataHeal[1];
                    if(_teamIndex == teamIndex)
                    {
                        if (currentLives == _maxLives) return;

                        currentLives = Math.Min(currentLives + healAmount, _maxLives);
                        healParticles.Play();
                    }
                    break;
                }
        }
    }

    #endregion
}
