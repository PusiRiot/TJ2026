using UnityEngine;

/// <summary>
/// This executes the lightning effect when a door closes, which happens on the closest point to where the door moved on the outside of the house
/// </summary>
public class DoorLightningEffect : MonoBehaviour
{
    #region Variables
    // variables needed to calc the lighning impact location
    [SerializeField] GameObject sceneFloor;
    Transform sceneFloorTransform;
    Vector2 meshSize;

    // variables needed for the visual effect
    [SerializeField] ParticleSystem lightningParticles;
    [SerializeField] Light lightningLight;

    #endregion

    void Awake()
    {
        sceneFloorTransform = GetComponent<Transform>();
        meshSize = Vector3.Scale(GetComponent<MeshFilter>().sharedMesh.bounds.size, sceneFloorTransform.localScale); // mesh size scaled by its transform
    }

    public void GenerateLighningEffect(Transform doorLocation)
    {
        Vector3 impactLocation = CalcImpactLocation(doorLocation);

    }

    Vector3 CalcImpactLocation(Transform doorLocation)
    {
        // Convert point to plane-local space
        Vector3 local = sceneFloorTransform.InverseTransformPoint(doorLocation.position);

        // We only need x and z axys
        float halfX = meshSize.x * 0.5f;
        float halfZ = meshSize.y * 0.5f;

        // Clamp inside the rectangle
        float clampedX = Mathf.Clamp(local.x, -halfX, halfX);
        float clampedZ = Mathf.Clamp(local.z, -halfZ, halfZ);

        // Distances to each edge
        float distLeft = Mathf.Abs(clampedX + halfX);
        float distRight = Mathf.Abs(halfX - clampedX);
        float distBottom = Mathf.Abs(clampedZ + halfZ);
        float distTop = Mathf.Abs(halfZ - clampedZ);

        // Find nearest edge
        float min = Mathf.Min(distLeft, distRight, distBottom, distTop);

        if (min == distLeft)
            clampedX = -halfX;
        else if (min == distRight)
            clampedX = halfX;
        else if (min == distBottom)
            clampedZ = -halfZ;
        else
            clampedZ = halfZ;

        // Convert back to world space
        return sceneFloorTransform.TransformPoint(new Vector3(clampedX, local.y, clampedZ));
    }
}
