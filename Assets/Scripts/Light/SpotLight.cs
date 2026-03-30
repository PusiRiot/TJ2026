using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents a spotlight that detects and interacts with crystals within its range and angle of view.
/// </summary>

public class SpotLight : AbstractLight
{
    [SerializeField] private float viewRange = 3f;
    [SerializeField] private float viewAngle = 40f;
    [SerializeField] private LayerMask detectCrystalMask;

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
                    if (Physics.Raycast(transform.position, dirToTarget, out RaycastHit rh, viewRange, detectCrystalMask))
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
                    if (Physics.Raycast(transform.position, dirToTarget, out RaycastHit rh, viewRange, detectCrystalMask))
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
