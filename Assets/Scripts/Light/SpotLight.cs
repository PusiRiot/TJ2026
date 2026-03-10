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
    private Dictionary<Crystal, float> detectionTimers = new ();
    private HashSet<Crystal> detectedThisFrame = new ();

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
            if (!hit.transform.TryGetComponent<Crystal>(out var crystal)) continue; // Skip if it's not a crystal

            Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;

            // Check angle
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle)
            {
                // Check line of sight
                if (Physics.Raycast(transform.position, dirToTarget, out RaycastHit rh, viewRange, detectCrystalMask))
                {
                    if (rh.collider == hit)
                    {
                        detectedThisFrame.Add(crystal);
                        
                        // increase or start timer for this crystal
                        if (detectionTimers.ContainsKey(crystal))
                        { 
                            // Check if the crystal can be captured
                            if(!crystal.CanCapture(teamIndex)) return;

                            detectionTimers[crystal] += Time.deltaTime;

                            if(crystal.ReclaimingPerforming(detectionTimers[crystal]))
                            {
                                crystal.ReclaimingPerformed(teamIndex);
                                detectionTimers[crystal] = 0f; // reset timer after lighting up
                            }
                        }
                        else
                        {
                            crystal.ReclaimingStarted();
                            detectionTimers[crystal] = Time.deltaTime;
                        }
                    }

                }
            }
        }

        // Reset timers for crystals not detected this frame
        foreach (var dictionaryCrystal in detectionTimers.Keys.ToList())
        {
            if (!detectedThisFrame.Contains(dictionaryCrystal))
            {
                dictionaryCrystal.ReclaimingCanceled();
                detectionTimers.Remove(dictionaryCrystal);
            }
        }

        detectedThisFrame.Clear();
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
