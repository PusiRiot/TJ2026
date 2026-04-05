using UnityEngine;
using UnityEngine.UIElements;

[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem))]
public class InsectParticles : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    ParticleSystem ps;
    ParticleSystemRenderer psRenderer;
    Light flashlight;

    private MaterialPropertyBlock propBlock;

    // This string must match the Reference string of the Matrix property in Shader Graph
    private static readonly int WorldToLocalMatrixID = Shader.PropertyToID("_WorldToLocal");
    private static readonly int LengthID = Shader.PropertyToID("_Length");

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        psRenderer = GetComponent<ParticleSystemRenderer>();
        propBlock = new MaterialPropertyBlock();
        flashlight = GetComponentInParent<Light>();
    }

    private void LateUpdate()
    {
        // If propBlock is null (e.g., after a script recompile), create it now.
        if (propBlock == null)
        {
            propBlock = new MaterialPropertyBlock();
        }

        // 1. Get the current property block so we don't overwrite other custom properties
        psRenderer.GetPropertyBlock(propBlock);
       
        // 2. Pass this GameObject's world-to-local matrix into the shader
        propBlock.SetMatrix(WorldToLocalMatrixID, transform.worldToLocalMatrix);

        float length = flashlight.range;

        // Check line of sight
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit rh, layerMask) && rh.distance < length)
        {
            length = rh.distance;
        }

        propBlock.SetFloat(LengthID, length);

        // 3. Apply the updated property block back to the renderer
        psRenderer.SetPropertyBlock(propBlock);
    }
}
