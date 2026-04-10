using System.Collections;
using UnityEngine;

/// <summary>
/// This executes the lightning effect when a door closes, which happens on the closest point to where the door moved on the outside of the house
/// </summary>
public class DoorLightningEffect : MonoBehaviour
{
    #region Variables
    // variables needed to calc the lighning impact location
    [SerializeField] MeshRenderer outsideBounds; // using the outside floor so it is further to the side

    // variables needed for the visual effect
    Light lightningLight;

    #endregion

    void Awake()
    {
        lightningLight = GetComponent<Light>();
        lightningLight.intensity = 0;
    }

    public void GenerateLighningEffect(Transform doorLocation)
    {
        this.transform.position = CalcImpactLocation(doorLocation);

        StartCoroutine(LightEffect());
    }

    IEnumerator LightEffect()
    {
        lightningLight.intensity = 10;
        yield return new WaitForSeconds(0.1f);
        lightningLight.intensity = 0;
        yield return new WaitForSeconds(0.1f);
        lightningLight.intensity = 10;
        yield return new WaitForSeconds(0.1f);
        lightningLight.intensity = 0;
    }

    Vector3 CalcImpactLocation(Transform doorLocation)
    {
        // Get world-space bounds of the plane
        Bounds b = outsideBounds.bounds;

        // Extract the 4 world-space edges
        float left = b.min.x;
        float right = b.max.x;
        float bottom = b.min.z;
        float top = b.max.z;

        // Get the point in world space
        Vector3 p = doorLocation.position;

        // Clamp X and Z inside the rectangle
        float clampedX = Mathf.Clamp(p.x, left, right);
        float clampedZ = Mathf.Clamp(p.z, bottom, top);

        // Distances to each edge
        float distLeft = Mathf.Abs(p.x - left);
        float distRight = Mathf.Abs(p.x - right);
        float distBottom = Mathf.Abs(p.z - bottom);
        float distTop = Mathf.Abs(p.z - top);

        // Snap to nearest edge
        float min = Mathf.Min(distLeft, distRight, distBottom, distTop);

        if (min == distLeft)
            clampedX = left;
        else if (min == distRight)
            clampedX = right;
        else if (min == distBottom)
            clampedZ = bottom;
        else
            clampedZ = top;

        // Return the final world-space point
        return new Vector3(clampedX, p.y + 2f, clampedZ);
    }
}
