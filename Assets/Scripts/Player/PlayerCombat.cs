using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : Subject<PlayerCombatEvent>
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

    private float _heavyMeleeLightOffDuration = -1;
    private float _succesfulParryLightOffDuration = -1;

    private float _deathDuration = -1f;

    private float _parryDuration = -1f;

    private float _lightMeleeCooldownDuration = -1f;
    private float _heavyMeleeCooldownDuration = -1f;
    private float _parryCooldownDuration = -1f;

    // Needed player references
    private PlayerMovement playerMovement;
    private Player player;
    private AbstractLight playerLight;

    // Visual effects
    private ParticleSystem parrySparks;
    private ParticleSystem parryingSparks;
    private ParticleSystem chargeSparks;
    private ParticleSystem attackSparks;
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

    public void Initialize(int teamIndex)
    {
        _teamIndex = teamIndex;

        _deathDuration = GameManager.Instance.DeathDuration();

        _maxLives = GameManager.Instance.GetMaxLives();
        currentLives = _maxLives;

        _lightMeleeDamage = GameManager.Instance.LightMeleeDamage();
        _heavyMeleeDamage = GameManager.Instance.HeavyMeleeDamage();

        _heavyMeleeLightOffDuration = GameManager.Instance.HeavyMeleeLightOffDuration();
        _succesfulParryLightOffDuration = GameManager.Instance.SuccesfulParryLightOffDuration();

        _parryDuration = GameManager.Instance.ParryDuration();

        _lightMeleeCooldownDuration = GameManager.Instance.LightMeleeCooldownDuration();
        _heavyMeleeCooldownDuration = GameManager.Instance.HeavyMeleeCooldownDuration();
        _parryCooldownDuration = GameManager.Instance.ParryCooldownDuration();
    }

    #endregion

    #region MonoBehaviour
    void Awake()
    {
        base.AddObserversOnScene();

        playerMovement = GetComponent<PlayerMovement>();

        player = GetComponent<Player>();

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
        }
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
        attackSparks.Play();

        if (isHeavyAttack)
        {
            // cooldown
            StartCoroutine(player.HeavyMeleeCooldown(_heavyMeleeCooldownDuration));

            InterruptCharge();
            StartCoroutine(HeavyAttack());
        }
        else
        {
            // cooldown
            StartCoroutine(player.LightMeleeCooldown(_lightMeleeCooldownDuration));

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
        // check collision
        Collider[] collisions = Physics.OverlapSphere(transform.position, 1f);

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
    void OnCollisionEnter(Collision collision)
    {
        if (!isAttackingHeavy) return;

        PlayerCombat enemy = collision.gameObject.GetComponentInParent<PlayerCombat>();
        if (enemy != null && enemy != this)
        {

            // effect
            bool succesful = enemy.ReceiveAttack(_heavyMeleeDamage, _heavyMeleeLightOffDuration, false);
            if (!succesful) ParryResponse();
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
    public IEnumerator Parry()
    {
        // cooldown
        StartCoroutine(player.ParryCooldown(_parryCooldownDuration));

        // effect
        parryingSparks.Play();
        isProtectedByParry = true;
        playerMovement.DisableMovement(true);

        yield return new WaitForSeconds(_parryDuration);

        parryingSparks.Stop();
        isProtectedByParry = false;
        playerMovement.DisableMovement(false);
    }

    public bool ReceiveAttack(int damage, float lightOffDuration, bool unableToParry)
    {
        if (isProtectedByParry && !unableToParry)
        {
            parrySparks.Play();
            return false;
        }

        // Light switching
        if (lightOffDuration > 0)
        {
            StopCoroutine(nameof(TurnLightOff));
            StartCoroutine(nameof(TurnLightOff), lightOffDuration);
        }

        // Interrupt player attacks
        if (isChargingAttack)
            player.CancelChargeAttack();


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
}
