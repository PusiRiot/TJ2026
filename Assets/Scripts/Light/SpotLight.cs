using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Represents a spotlight that detects and interacts with crystals within its range and angle of view.
/// </summary>

public class SpotLight : AbstractLight
{
    [SerializeField] private float viewRange = 3f;
    [SerializeField] private float viewAngle = 40f;
    [SerializeField] private LayerMask ignorePlayerMask;
    [SerializeField] private LayerMask ignoreCrystalMask;

    private AbstractAbility lifeDrainAbility;

    private void Start()
    {
        lifeDrainAbility = GetComponentInParent<AbstractAbility>();
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
            // Skip if it's not a crystal
            if (hit.transform.TryGetComponent<Crystal>(out var crystal))
            {
                Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;

                // Check angle
                if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle)
                {
                    // Check line of sight
                    if (Physics.Raycast(transform.position, dirToTarget, out RaycastHit rh, viewRange, ignorePlayerMask))
                    {
                        if (rh.collider == hit)
                        {
                            crystal.ReclaimFlag(teamIndex);
                        }
                    }
                }
            }
            else if(hit.transform.TryGetComponent<Heal>(out var heal))
            {
                Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;

                // Check angle
                if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle)
                {
                    // Check line of sight
                    //TODO CHANGE LAYER TO HEAL LAYER
                    if (Physics.Raycast(transform.position, dirToTarget, out RaycastHit rh, viewRange, ignorePlayerMask))
                    {
                        if (rh.collider == hit)
                        {
                            heal.ReclaimFlag(teamIndex);
                        }
                    }
                }
            }
        }
    }

    public void ActivateLifeDrain(PlayerStats playerStats)
    {
        // Life Drain Visuals
        flashlight.color = Color.red; // Example: change light color to red when activating Life Drain
        StartCoroutine(LifeDrainCoroutine(playerStats));
    }

    public void StopLifeDrain()
    {
        StopCoroutine(nameof(LifeDrainCoroutine));
        //Life Drain deactivation visuals
        flashlight.color = GameManager.Instance.GetTeamColor(teamIndex);
        lifeDrainAbility.StartCooldown();
    }

    private IEnumerator LifeDrainCoroutine(PlayerStats playerStats)
    {
        int pulsesRemaining = playerStats.LifeDrainNumPulses;
        LifeDrainPulse(playerStats);
        pulsesRemaining--;

        while (pulsesRemaining > 0)
        {
            yield return new WaitForSeconds(playerStats.LifeDrainPulseCadence);

            pulsesRemaining--;
            LifeDrainPulse(playerStats);
        }

        StopLifeDrain();
    }
    

    private void LifeDrainPulse(PlayerStats playerStats)
    {
        // Pulse visuals
        StartCoroutine(TempPulseVisuals());

        if (DetectLifeDrainCollision())
        {
            Notify(PlayerCombatEvent.ReceivedHeal, new int[] { teamIndex, playerStats.LifeDrainPulseHeal });
            Notify(PlayerCombatEvent.ReceivedDamage, new int[] { (teamIndex + 1) % 2, playerStats.LifeDrainPulseDamage });
        }
    }

    private bool DetectLifeDrainCollision()
    {
        // Get all colliders inside a sphere around the player with a radius of viewRange
        Collider[] hits = Physics.OverlapSphere(transform.position, viewRange);
        if (hits.Length == 0) return false; // No colliders in range, skip

        foreach (var hit in hits)
        {
            Player hitPlayer = hit.GetComponentInParent<Player>();

            // Skip if it's not a player
            if (hitPlayer != null && hitPlayer.GetTeamIndex() != GetComponentInParent<Player>().GetTeamIndex())
            {

                Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;

                // Check angle
                if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle)
                {
                    // Check line of sight
                    if (Physics.Raycast(transform.position, dirToTarget, out RaycastHit rh, viewRange, ignoreCrystalMask))
                    {
                        if (rh.collider == hit)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
    //TODO: add finals visuals
    private IEnumerator TempPulseVisuals()
    {
        flashlight.intensity *= 5;
        yield return new WaitForSeconds(0.2f);
        flashlight.intensity /= 5;
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
