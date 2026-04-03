using UnityEngine;

/// <summary>
/// Represents a spotlight that detects and interacts with crystals within its range and angle of view.
/// </summary>

public class AreaLight : AbstractLight
{
    [SerializeField] private float viewRange = 3f;
    [SerializeField] private LayerMask ignorePlayerMask;
    [SerializeField] private LayerMask justCrystalHealMask;

    /// <summary>
    /// Detect if any crystals are within the spotlight's range and angle, and if there is a clear line of sight to them. If so, call to crystal method to light it up.
    /// </summary>
    protected override void DetectLightCollision()
    {
        // Get all colliders inside a sphere around the player with a radius of viewRange
        Collider[] hits = Physics.OverlapSphere(transform.position, viewRange, justCrystalHealMask);
        if (hits.Length == 0) return; // No colliders in range, skip

        foreach (var hit in hits)
        {
            Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;

            // Check line of sight
            if (Physics.Raycast(transform.position, dirToTarget, out RaycastHit rh, viewRange, ignorePlayerMask))
            {
                if (rh.collider != hit)
                    continue; // Something is blocking the line of sight, skip

                // Skip if it's not a crystal
                if (hit.transform.TryGetComponent<Crystal>(out var crystal))
                {
                    crystal.ReclaimFlag(teamIndex);
                }
                else if(hit.transform.TryGetComponent<Heal>(out var heal))
                {
                    heal.ReclaimFlag(teamIndex);
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
    }
}
