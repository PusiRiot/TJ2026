using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    private float _lightMeleeRange = -1;
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
    // Visual effects
    private ParticleSystem stunBurstParticles;
    private ParticleSystem stunIdleParticles;
    private ParticleSystem parryingSparks;
    private ParticleSystem chargeSparks;
    private ParticleSystem attackSparks;
    private ParticleSystem healParticles;

    // Groovy outline
    [SerializeField] private Material transparentMask; 
    private List<Material> groovyOutlineMaterials = new List<Material>();
    private float currentThickness = 0f;
    private float targetThickness = 0f;
    private float interpolationSpeed = 0f;
    private const float desiredThickness = 0.05f;
    private Color outlineColor;

    // Death materials
    private List<Material> deathOutlineMaterials;

    // Boolean control
    private bool isProtectedByParry = false;
    private bool isAttackingHeavy = false;
    private bool isChargingAttack = false;
    private bool alreadyHit = false; // to prevent hitting multiple times with the heavy melee dash
    private bool isDead = false;

    //Audio
    private PlayerSFX playerSFX;
    public void Initialize(int teamIndex)
    {
        _teamIndex = teamIndex;

        _deathDuration = GameStatsAccess.Instance.DeathDuration();

        _maxLives = GameStatsAccess.Instance.GetMaxLives();
        currentLives = _maxLives;

        _glowUpDuration = GameStatsAccess.Instance.GetGroovyOutlineGlowUp();
        _glowDownDuration = GameStatsAccess.Instance.GetGroovyOutlineGlowDown();

        _lightMeleeDamage = GameStatsAccess.Instance.LightMeleeDamage();
        _lightMeleeRange = GameStatsAccess.Instance.LightMeleeRange();
        _heavyMeleeDamage = GameStatsAccess.Instance.HeavyMeleeDamage();

        _heavyMeleeDashDuration = GameStatsAccess.Instance.HeavyMeleeDashDuration();
        _heavyMeleeDashSpeedIncrement = GameStatsAccess.Instance.HeavyMeleeDashSpeedIncrement();

        _heavyMeleeLightOffDuration = GameStatsAccess.Instance.HeavyMeleeLightOffDuration();
        _heavyMeleeStunDuration = GameStatsAccess.Instance.HeavyMeleeStunDuration();
        _succesfulParryLightOffDuration = GameStatsAccess.Instance.SuccesfulParryLightOffDuration();

        _parryDuration = GameStatsAccess.Instance.ParryDuration();
        _succesfulParryStunDuration = GameStatsAccess.Instance.SuccesfulParryStunDuration();
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
        foreach (ParticleSystem particle in particles)
        {
            if (particle.gameObject.CompareTag("StunBurstParticles"))
            {
                stunBurstParticles = particle;
                Debug.Log(player.GetTeamIndex() + " Tag: StunBurstParticles, Name: " + particle.gameObject.name);
            }
            else if (particle.gameObject.CompareTag("StunIdleParticles"))
            {
                stunIdleParticles = particle;
                Debug.Log(player.GetTeamIndex() + " Tag: StunIdleParticles, Name: " + particle.gameObject.name);
            }
            else if (particle.gameObject.CompareTag("ChargeAttackParticles"))
            {
                chargeSparks = particle;
                Debug.Log(player.GetTeamIndex() + " Tag: ChargeAttackParticles, Name: " + particle.gameObject.name);
            }
            else if (particle.gameObject.CompareTag("AttackParticles"))
            {
                attackSparks = particle;
                Debug.Log(player.GetTeamIndex() + " Tag: AttackParticles, Name: " + particle.gameObject.name);
            }
            else if (particle.gameObject.CompareTag("ParryingParticles"))
            {
                parryingSparks = particle;
                Debug.Log(player.GetTeamIndex() + " Tag: ParryingParticles, Name: " + particle.gameObject.name);
            }
            else if (particle.gameObject.CompareTag("HealParticles"))
            {
                healParticles = particle;
                Debug.Log(player.GetTeamIndex() + " Tag: HealParticles, Name: " + particle.gameObject.name);
            }
        }

        // Groovy outline materials initialization
        SkinnedMeshRenderer[] playerMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer playerMesh in playerMeshes)
        {
            foreach(Material mat in playerMesh.materials)
            {
                if (mat.HasFloat("_BaseThickness"))
                {
                    groovyOutlineMaterials.Add(mat);
                    mat.SetFloat("_BaseThickness", 0.0f);
                    mat.SetColor("_OutlineColor", new Color(0.0f, 0.0f, 0.0f, 0.0f));
                }
            }
        }

        // Death materials
        deathOutlineMaterials = new List<Material>();

        foreach (Material originalMat in groovyOutlineMaterials)
        {
            // This creates a brand new instance in memory with the same properties
            Material clonedMat = new Material(originalMat);
            deathOutlineMaterials.Add(clonedMat);
        }

        //Audio
        playerSFX = GetComponent<PlayerSFX>();

    }

void FixedUpdate()
    {
        if(currentThickness != targetThickness)
        {
            currentThickness = Mathf.MoveTowards(currentThickness, targetThickness, interpolationSpeed * Time.fixedDeltaTime);
            foreach (Material mat in groovyOutlineMaterials)
            {
                mat.SetFloat("_BaseThickness", currentThickness);
            }
        }
    }

    void OnDestroy()
    {
        foreach (Material mat in deathOutlineMaterials)
        {
            Destroy(mat);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!isAttackingHeavy) return;

        PlayerCombat enemy = other.gameObject.GetComponentInParent<PlayerCombat>();
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

    #region Groovy outline methods

    private IEnumerator AnimateGroovyOutline(Color inOutlineColor)
    {
        outlineColor = inOutlineColor;
        interpolationSpeed = desiredThickness / _glowUpDuration;
        currentThickness = 0.0f;
        targetThickness = desiredThickness;

        foreach (Material mat in groovyOutlineMaterials)
        {
            mat.SetColor("_OutlineColor", outlineColor);
            mat.SetFloat("_BaseThickness", 0.0f);
        }

        yield return new WaitForSeconds(_glowUpDuration);

        interpolationSpeed = desiredThickness / _glowDownDuration;
        currentThickness = desiredThickness;
        targetThickness = 0.0f;

        foreach (Material mat in groovyOutlineMaterials)
        {
            mat.SetFloat("_BaseThickness", desiredThickness);
        }

        yield return new WaitForSeconds(_glowDownDuration);

        interpolationSpeed = 0f;
        currentThickness = targetThickness;

        foreach (Material mat in groovyOutlineMaterials)
        {
            mat.SetFloat("_BaseThickness", 0.0f);
            mat.SetColor("_OutlineColor", new Color(0.0f, 0.0f, 0.0f, 0.0f));
        }
    }

    #endregion

    #region Attack methods
    public void ExecuteAttack(bool isHeavyAttack)
    {
        if (isHeavyAttack)
        {
            //Audio
            AkUnitySoundEngine.PostEvent("Play_Heavy", gameObject);

            attackSparks.Play();

            InterruptCharge();
            StartCoroutine(HeavyAttack());
        }
        else
        {
            //Audio
            AkUnitySoundEngine.PostEvent("Play_Light", gameObject);

            StartCoroutine(LightAttack());
        }
    }

    IEnumerator HeavyAttack()
    {
        playerAnimator.TriggerHeavyAttack();

        //dash
        isProtectedByParry = true;
        isAttackingHeavy = true;
        alreadyHit = false;
        playerMovement.Dash(_heavyMeleeDashDuration, _heavyMeleeDashSpeedIncrement);

        // after dash player is no longer attacking
        yield return new WaitForSeconds(_heavyMeleeDashDuration);
        isAttackingHeavy = false;
        isProtectedByParry = false;
        playerAnimator.CancelAttack();
    }

    IEnumerator LightAttack()
    {
        playerAnimator.TriggerLightAttack();
        StartCoroutine(playerMovement.DisableMovement(0.4f));

        yield return new WaitForSeconds(0.2f); //TODO: to sync with animation, can be changed later when we have the final one
        attackSparks.Play();

        // check collision
        float range = _lightMeleeRange;
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
        isProtectedByParry = false;
        playerMovement.DisableMovement(false);
    }

    public bool ReceiveHeavyMelee()
    {
        if (isProtectedByParry)
        {
            StopParry(true);
            return false;
        }

        // disable movement shortly for hit stun
        StartCoroutine(Stun(_heavyMeleeStunDuration, false));

        // animation
        playerAnimator.TriggerStunAttack();

        StopCoroutine(nameof(TurnLightOff));
        StartCoroutine(nameof(TurnLightOff), _heavyMeleeLightOffDuration);

        StopCoroutine(LightAttack());

        //Audio
        playerSFX.PlayHurt();

        // Damage
        Notify(PlayerCombatEvent.ReceivedDamage, new int[]{_teamIndex, _heavyMeleeDamage});

        return true;
    }

    public void ReceiveLightMelee()
    {
        playerAnimator.TriggerLightDamage();

        //Audio
        playerSFX.PlayHurt();

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

        StopCoroutine(LightAttack());
    }

    IEnumerator Death()
    {
        Notify(PlayerCombatEvent.Death, _teamIndex);
        currentLives = _maxLives;

        isDead = true;
        // disable actions and world interaction
        player.DisableWorldInteraction();
        playerAbility.Stop();
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
        foreach (Material mat in deathOutlineMaterials)
        {
            Color teamColor = GameStatsAccess.Instance.GetTeamColor(_teamIndex);
            mat.SetColor("_OutlineColor", teamColor);
            mat.SetFloat("_BaseThickness", desiredThickness); 
        }

        for (int i = 0; i < playerMeshes.Length; i++)
        {
            Material[] deathMaterial = { transparentMask, deathOutlineMaterials[i] };
            playerMeshes[i].sharedMaterials = deathMaterial;
        }

        Debug.Log("stopped coroutine");
        //Audio
        playerSFX.PlayTurnOff();
        playerLight.TurnOff(); // don't call method to not start twice the same coroutine

        Debug.Log(_deathDuration);
        yield return new WaitForSeconds(_deathDuration);

        // light switching
        //Audio
        playerSFX.PlayTurnOn();
        playerLight.TurnOn();

        // enable actions and world interaction
        player.EnableWorldInteraction();
        isDead = false;

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

        // Interrupt player attacks
        if (isChargingAttack)
            player.CancelChargeAttack();

        //Audio
        AkUnitySoundEngine.PostEvent("Play_Stunned", gameObject);

        stunBurstParticles.Play();

        if(duration > 0.25f)
        {
            stunIdleParticles.Play();
        }

        parryingSparks.Stop();
        chargeSparks.Stop();
        attackSparks.Stop();
        playerAnimator.CancelChargeAttack();
        playerAnimator.CancelAttack();

        playerMovement.DisableMovement(true);
        playerMovement.ToggleRotation(false);
        playerAbility.Stop();
        player.DisablePlayerActions();

        yield return new WaitForSeconds(duration);

        stunIdleParticles.Stop();

        if (!isDead)
        {
            player.EnablePlayerActions();
        }
        playerMovement.DisableMovement(false);
        playerMovement.ToggleRotation(true);

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
                    StartCoroutine(AnimateGroovyOutline(GameStatsAccess.Instance.GetHealColor()));
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
                    StartCoroutine(AnimateGroovyOutline(GameStatsAccess.Instance.GetDamageColor()));

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
