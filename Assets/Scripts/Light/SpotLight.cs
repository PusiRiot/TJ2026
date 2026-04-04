using System.Collections;
using UnityEngine;

/// <summary>
/// Represents a spotlight that detects and interacts with crystals within its range and angle of view.
/// </summary>

public class SpotLight : AbstractLight
{
    [SerializeField] private float viewRange = 3f;
    [SerializeField] private float viewAngle = 40f;
    [SerializeField] private LayerMask ignorePlayerMask;
    [SerializeField] private LayerMask ignoreCrystalMask;
    //[SerializeField] private LayerMask justCrystalHeal;

    [Header("Life Drain VFX")]
    [SerializeField] private float regularParticlesEmission = 40.0f;
    [SerializeField] private float pulseVisualsDuration = 0.5f;
    [SerializeField] private float pulseIntensityMultiplier = 2.0f;
    [SerializeField] private float pulseAnimationDuration = 0.1f;
    [SerializeField] private float pulseParticlesStartLifetime = 0.7f;
    [SerializeField] private float pulseParticlesStartSpeed = -12.0f;
    [SerializeField] private float pulseParticlesEmission = 225.0f;

    private ParticleSystem lifeDrainParticles;
    private PlayerStats playerStats;

    private bool isPulsing = false;
    private bool alreadyDamageThisPulse = false;   

    private float initialPsStartLifetime;
    private float initialPsStartSpeed;

    private float pulseAnimationRate;
    private Color targetColor;
    private float targetIntensity;

    private void Start()
    {
        lifeDrainParticles = GetComponentInChildren<ParticleSystem>();
      
        initialPsStartLifetime = lifeDrainParticles.main.startLifetime.constant;
        initialPsStartSpeed = lifeDrainParticles.main.startSpeed.constant;

        pulseAnimationRate = 1f / pulseAnimationDuration;
        targetColor = flashlight.color;
        targetIntensity = flashlight.intensity;
    }

    private void FixedUpdate()
    {
        if(Vector4.Distance(flashlight.color, targetColor) > 0.01f)
        {
            flashlight.color = Color.Lerp(flashlight.color, targetColor, pulseAnimationRate * Time.fixedDeltaTime);
        }

        if(Mathf.Abs(flashlight.intensity - targetIntensity) > 0.01f)
        {
            flashlight.intensity = Mathf.Lerp(flashlight.intensity, targetIntensity, pulseAnimationRate * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// Detect if any crystals are within the spotlight's range and angle, and if there is a clear line of sight to them. If so, call to crystal method to light it up.
    /// </summary>
    protected override void DetectLightCollision()
    {
        // Get all colliders inside a sphere around the player with a radius of viewRange
        Collider[] hits = Physics.OverlapSphere(transform.position, viewRange);
        if (hits.Length == 0) return; // No colliders in range, skip

        foreach (var hit in hits)
        {
            Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;

            if (Physics.Raycast(transform.position, dirToTarget, out RaycastHit rh, viewRange, ignorePlayerMask))
            {
                if(rh.collider != hit)
                    continue;
                // Skip if it's not a crystal
                if (hit.transform.TryGetComponent<Crystal>(out var crystal))
                {
                    
                    // Check angle
                    if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle)
                    {
                        crystal.ReclaimFlag(teamIndex);
                    }
                }
                else if(hit.transform.TryGetComponent<Heal>(out var heal))
                {
                    // Check angle
                    if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle)
                    {
                        // Check line of sight
                        // DONE by Adri
                        if (rh.collider == hit)
                        {
                            heal.ReclaimFlag(teamIndex);
                        }

                    }
                }
            }

            if (isPulsing && !alreadyDamageThisPulse)
            {
                if (Physics.Raycast(transform.position, dirToTarget, out RaycastHit rhPlayer, viewRange, ignoreCrystalMask))
                {
                    Player hitPlayer = hit.GetComponentInParent<Player>();

                    if (hitPlayer != null && hitPlayer.GetTeamIndex() != GetComponentInParent<Player>().GetTeamIndex())
                    {
                        // Check angle
                        if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle)
                        {
                            if (rhPlayer.collider == hit)
                            {
                                Notify(PlayerCombatEvent.ReceivedHeal, new int[] { teamIndex, playerStats.LifeDrainPulseHeal });
                                Notify(PlayerCombatEvent.ReceivedDamage, new int[] { (teamIndex + 1) % 2, playerStats.LifeDrainPulseDamage });
                                alreadyDamageThisPulse = true;
                            }

                        }
                    }
                }
            }
        }
    }

    public void ActivateLifeDrain(PlayerStats inPlayerStats)
    {
        playerStats = inPlayerStats;
        // Life Drain Visuals
        var psEmission = lifeDrainParticles.emission;
        psEmission.rateOverTime = regularParticlesEmission;
        StartCoroutine(LifeDrainCoroutine());
    }

    public void StopLifeDrain()
    {
        StopAllCoroutines();
        //Life Drain deactivation visuals
        var psMain = lifeDrainParticles.main;
        var psEmission = lifeDrainParticles.emission;
        psEmission.rateOverTime = 0.0f;
        psMain.startLifetime = initialPsStartLifetime;
        psMain.startSpeed = initialPsStartSpeed;
        targetColor = GameManager.Instance.GetTeamColor(teamIndex);
        flashlight.color = GameManager.Instance.GetTeamColor(teamIndex);
        isPulsing = false;
        Notify(PlayerCombatEvent.StartAbilityCooldown, new int[] { teamIndex });
    }

    private IEnumerator LifeDrainCoroutine()
    {
        int pulsesRemaining = playerStats.LifeDrainNumPulses;
        //Pulse
        alreadyDamageThisPulse = false;
        StartCoroutine(LifeDrainPulseVisuals());
        pulsesRemaining--;

        while (pulsesRemaining > 0)
        {
            yield return new WaitForSeconds(playerStats.LifeDrainPulseCadence);

            //Pulse
            alreadyDamageThisPulse = false;
            StartCoroutine(LifeDrainPulseVisuals());
            pulsesRemaining--;
        }

        yield return new WaitForSeconds(pulseVisualsDuration + pulseAnimationDuration); // Wait a bit before turning off particles to let the last pulse visuals play out

        StopLifeDrain();
    }

    private IEnumerator LifeDrainPulseVisuals() 
    {
        isPulsing = true;
        targetColor = GameManager.Instance.GetDamageColor();
        targetIntensity *= pulseIntensityMultiplier;
        var psMain = lifeDrainParticles.main;
        psMain.startLifetime = pulseParticlesStartLifetime;
        psMain.startSpeed = pulseParticlesStartSpeed;

        var psEmission = lifeDrainParticles.emission;
        psEmission.rateOverTime = pulseParticlesEmission;

        yield return new WaitForSeconds(pulseVisualsDuration);

        psEmission.rateOverTime = regularParticlesEmission;
        psMain.startLifetime = initialPsStartLifetime;
        psMain.startSpeed = initialPsStartSpeed;
        targetIntensity /= pulseIntensityMultiplier;
        targetColor = GameManager.Instance.GetTeamColor(teamIndex);
        isPulsing = false;
    }

    /// <summary>
    /// To visualize the spotlight's range and angle in the editor
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // Sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRange);

        // Cone edges
        Gizmos.color = Color.cyan;
        Vector3 left = Quaternion.Euler(0, -viewAngle, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, left * viewRange);
        Gizmos.DrawRay(transform.position, right * viewRange);
    }
}
