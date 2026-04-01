using UnityEngine;

/// <summary>
/// Represents a spotlight that detects and interacts with crystals within its range and angle of view.
/// </summary>

public class AreaLight : AbstractLight
{
    [SerializeField] private float viewRange = 3f;

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
                crystal.ReclaimFlag(teamIndex);
            }
            else if(hit.transform.TryGetComponent<Heal>(out var heal))
            {
                heal.ReclaimFlag(teamIndex);
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
