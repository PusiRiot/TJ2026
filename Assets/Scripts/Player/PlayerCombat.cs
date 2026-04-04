using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private float _heavyMeleeStunDuration = -1;
    private float _succesfulParryLightOffDuration = -1;
    private float _succesfulParryStunDuration = -1;

    private float _deathDuration = -1f;

    private float _parryDuration = -1f;

    private float _glowUpDuration = -1f;
    private float _glowDownDuration = -1f;

    // Needed player references
    private PlayerMovement playerMovement;
    private Player player;
    private AbstractLight playerLight;
    private PlayerAnimator playerAnimator;
    private AbstractAbility playerAbility;

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

    // Glow overlay
    private List<Material> glowOverlayMaterials = new List<Material>();
    private float currentAlpha = 0f;
    private float targetAlpha = 0f;
    private float interpolationSpeed = 0f;
    private Color glowColor;

    // Death materials
    private List<Material> deathGlowMaterials;

    // Boolean control
    private bool isProtectedByParry = false;
    private bool isAttackingHeavy = false;
    private bool isChargingAttack = false;
    private bool alreadyHit = false; // to prevent hitting multiple times with the heavy melee dash

    public void Initialize(int teamIndex)
    {
        _teamIndex = teamIndex;

        _deathDuration = GameManager.Instance.DeathDuration();

        _maxLives = GameManager.Instance.GetMaxLives();
        currentLives = _maxLives;

        _glowUpDuration = GameManager.Instance.GetGlowOverlayGlowUp();
        _glowDownDuration = GameManager.Instance.GetGlowOverlayGlowDown();

        _lightMeleeDamage = GameManager.Instance.LightMeleeDamage();
        _heavyMeleeDamage = GameManager.Instance.HeavyMeleeDamage();

        _heavyMeleeDashDuration = GameManager.Instance.HeavyMeleeDashDuration();
        _heavyMeleeDashSpeedIncrement = GameManager.Instance.HeavyMeleeDashSpeedIncrement();

        _heavyMeleeLightOffDuration = GameManager.Instance.HeavyMeleeLightOffDuration();
        _heavyMeleeStunDuration = GameManager.Instance.HeavyMeleeStunDuration();
        _succesfulParryLightOffDuration = GameManager.Instance.SuccesfulParryLightOffDuration();

        _parryDuration = GameManager.Instance.ParryDuration();
        _succesfulParryStunDuration = GameManager.Instance.SuccesfulParryStunDuration();
    }

    #endregion

    #region MonoBehaviour
    void Awake()
    {
        base.AddObserversOnScene();

        playerMovement = GetComponent<PlayerMovement>();

        player = GetComponent<Player>();

        playerAnimator = GetComponent<PlayerAnimator>();

        playerAbility = GetComponent<AbstractAbility>();

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

        // Glow overlay materials initialization
        SkinnedMeshRenderer[] playerMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer playerMesh in playerMeshes)
        {
            foreach(Material mat in playerMesh.materials)
            {
                if (mat.HasFloat("_Fresnel"))
                {
                    glowOverlayMaterials.Add(mat);
                    mat.SetColor("_Color", new Color(0,0,0,0));
                }
            }
        }

        // Death materials
        deathGlowMaterials = new List<Material>();

        foreach (Material originalMat in glowOverlayMaterials)
        {
            // This creates a brand new instance in memory with the same properties
            Material clonedMat = new Material(originalMat);
            deathGlowMaterials.Add(clonedMat);
        }
    }

    void FixedUpdate()
    {
        if(targetAlpha != currentAlpha)
        {
            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, interpolationSpeed * Time.fixedDeltaTime);
            foreach (Material mat in glowOverlayMaterials)
            {
                mat.SetColor("_Color", new Color(glowColor.r, glowColor.g, glowColor.b, currentAlpha));
            }
        }
    }

    void OnDestroy()
    {
        foreach (Material mat in deathGlowMaterials)
        {
            Destroy(mat);
        }
    }

    #endregion

    #region Glow overlay methods

    private IEnumerator AnimateGlowOverlay(Color inGlowColor)
    {
        glowColor = inGlowColor;
        interpolationSpeed = 1f / _glowUpDuration;
        currentAlpha = 0.0f;
        targetAlpha = 1.0f;

        foreach (Material mat in glowOverlayMaterials)
        {
            mat.SetColor("_Color", new Color(glowColor.r, glowColor.g, glowColor.b, currentAlpha));
        }

        yield return new WaitForSeconds(_glowUpDuration);

        interpolationSpeed = 1f / _glowDownDuration;
        currentAlpha = 1.0f;
        targetAlpha = 0.0f;

        foreach (Material mat in glowOverlayMaterials)
        {
            mat.SetColor("_Color", new Color(glowColor.r, glowColor.g, glowColor.b, currentAlpha));
        }

        yield return new WaitForSeconds(_glowDownDuration);

        interpolationSpeed = 0f;
        currentAlpha = targetAlpha;

        foreach (Material mat in glowOverlayMaterials)
        {
            mat.SetColor("_Color", new Color(0,0,0,0));
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
            if (enemy != null && enemy != this && enemy.enabled)
            {
                // effect
                enemy.ReceiveLightMelee();
            }
        }
    }
   
    void OnCollisionStay(Collision collision)
    {
        if (!isAttackingHeavy) return;

        PlayerCombat enemy = collision.gameObject.GetComponentInParent<PlayerCombat>();
        if (enemy != null && enemy != this && !alreadyHit && enemy.enabled)
        {
            isAttackingHeavy = false; // to avoid multiple collisions on the same attack
            // effect
            bool succesful = enemy.ReceiveHeavyMelee();
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

    public bool ReceiveHeavyMelee()
    {
        if (isProtectedByParry)
        {
            parrySparks.Play();
            StopParry(true);
            return false;
        }

        // disable movement shortly for hit stun
        StartCoroutine(Stun(_heavyMeleeStunDuration, false));

        // animation
        playerAnimator.TriggerStunAttack();

        StopCoroutine(nameof(TurnLightOff));
        StartCoroutine(nameof(TurnLightOff), _heavyMeleeLightOffDuration);

        // Interrupt player attacks
        if (isChargingAttack)
            player.CancelChargeAttack();

        playerAbility.Stop();
        StopCoroutine(LightAttack());

        // Damage
        Notify(PlayerCombatEvent.ReceivedDamage, new int[]{_teamIndex, _heavyMeleeDamage});

        return true;
    }

    public void ReceiveLightMelee()
    {
        playerAnimator.TriggerLightDamage();

        // Damage
        Notify(PlayerCombatEvent.ReceivedDamage, new int[] { _teamIndex, _lightMeleeDamage });
    }

    void ParryResponse()
    {
        playerAnimator.TriggerStun();

        // light switching
        StopCoroutine(nameof(TurnLightOff)); // in case another coroutine is up
        StartCoroutine(nameof(TurnLightOff), _succesfulParryLightOffDuration); // only coroutines started by name can be stopped by name

        // stun for one second
        StartCoroutine(Stun(_succesfulParryStunDuration, true));

        // Interrupt player attacks
        if (isChargingAttack)
            player.CancelChargeAttack();

        playerAbility.Stop();
        StopCoroutine(LightAttack());
    }

    IEnumerator Death()
    {
        Notify(PlayerCombatEvent.Death, _teamIndex);
        currentLives = _maxLives;

        // disable actions and world interaction
        player.DisableWorldInteraction();

        // light switching
        StopCoroutine(nameof(TurnLightOff)); // in case another coroutine is up

        // Store current state
        List<Material[]> currentMaterialsCopy = new List<Material[]>();
        SkinnedMeshRenderer[] playerMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (SkinnedMeshRenderer playerMesh in playerMeshes)
        {
            currentMaterialsCopy.Add(playerMesh.sharedMaterials);
        }

        // Apply the new death materials
        foreach (Material mat in deathGlowMaterials)
        {
            Color teamColor = GameManager.Instance.GetTeamColor(_teamIndex);
            teamColor.a = 0.4f; // Set the alpha to 0.5 for a semi-transparent effect
            mat.SetColor("_Color", teamColor);
            mat.SetFloat("_Fresnel", 0.75f); 
        }

        for (int i = 0; i < playerMeshes.Length; i++)
        {
            Material[] deathMaterial = { deathGlowMaterials[i] };
            playerMeshes[i].sharedMaterials = deathMaterial;
        }

        Debug.Log("stopped coroutine");
        playerLight.TurnOff(); // don't call method to not start twice the same coroutine

        Debug.Log(_deathDuration);
        yield return new WaitForSeconds(_deathDuration);

        // light switching
        playerLight.TurnOn();

        // enable actions and world interaction
        player.EnableWorldInteraction();

        // Revert materials back to the original ones
        for (int i = 0; i < playerMeshes.Length; i++)
        {
            playerMeshes[i].sharedMaterials = currentMaterialsCopy[i];
        }

        Notify(PlayerCombatEvent.BackToLife, _teamIndex);
    }

    IEnumerator TurnLightOff(float duration)
    {
        playerLight.TurnOff();
        yield return new WaitForSeconds(duration);
        playerLight.TurnOn();

    }

    IEnumerator Stun(float duration, bool enableAnimation)
    {
        if (enableAnimation)
            playerAnimator.TriggerStun();

        playerMovement.DisableMovement(true);
        player.DisablePlayerActions();

        yield return new WaitForSeconds(duration);
        playerMovement.DisableMovement(false);
        player.EnablePlayerActions();

        if (enableAnimation)
            playerAnimator.CancelStun();
    }

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
                if (_teamIndex == teamIndex)
                {
                    if (currentLives == _maxLives) return;

                    currentLives = Math.Min(currentLives + healAmount, _maxLives);
                    healParticles.Play();
                    StartCoroutine(AnimateGlowOverlay(GameManager.Instance.GetHealColor()));
                }
                break;
            }
            case PlayerCombatEvent.ReceivedDamage:
            {
                int[] dataDamage = data as int[];
                int teamIndex = dataDamage[0];
                int damageAmount = dataDamage[1];
                if (_teamIndex == teamIndex)
                {
                    // Damage
                    currentLives -= damageAmount;
                    StartCoroutine(AnimateGlowOverlay(GameManager.Instance.GetDamageColor()));

                    if (currentLives <= 0)
                    {
                        StartCoroutine(nameof(Death));
                    }
                }
                break;
            }
        }
    }

    #endregion
}
