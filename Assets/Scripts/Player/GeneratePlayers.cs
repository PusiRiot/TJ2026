using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Script to generate players on runtime when game starts based on the player selection of GameGlobalSettings
/// </summary>
public class GeneratePlayers : MonoBehaviour
{
    [SerializeField] GameObject[] players = new GameObject[2];
    [SerializeField] Material[] parryMaterials = new Material[2];
    [SerializeField] Transform[] startPosition = new Transform[2];

    private void Awake()
    {
        for (int i = 0; i < 2; i++)
        {
            PlayerCharacter character = GameGlobalSettings.Instance.GetPlayerCharacter(i);

            // get player with character
            foreach (GameObject p in players)
            {
                if (p.GetComponent<Player>().GetPlayerCharacter() == character)
                {
                    p.SetActive(false); // IMPORTANT: instanciate disabled so scripts only awake after player tag its assigned
                    GameObject instanciated = Instantiate(p);
                    instanciated.transform.position = startPosition[i].position;
                    instanciated.tag = i == 0 ? "Player1" : "Player2";
                    ConfigureParryMaterials(instanciated, i);
                    ConfigCharacterLayer(instanciated, i);
                    ConfigCharacterRenderLayerMask(instanciated, i);
                    ConfigLightsRenderLayerMask(instanciated, i);
                    instanciated.SetActive(true);
                    break;
                }
            }
        }
    }

    private void ConfigureParryMaterials(GameObject player, int teamIndex)
    {
        // Set the parry material of the player's renderers to the appropriate team material
        Material parryMaterial = parryMaterials[teamIndex];
        MeshRenderer[] meshRenderers = player.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            if (meshRenderer.gameObject.CompareTag("ParrySphere"))
            {
                meshRenderer.material = parryMaterial;
            }
        }
    }

    private void ConfigCharacterLayer(GameObject player, int teamIndex)
    {
        // Set the layer of the player and its children to the appropriate team layer
        int layer = teamIndex == 0 ? LayerMask.NameToLayer("Player1") : LayerMask.NameToLayer("Player2");
        player.layer = layer;

        var children = player.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (var child in children)
        {
            child.gameObject.layer = layer;
        }
    }

    private void ConfigLightsRenderLayerMask(GameObject player, int teamIndex)
    {
        // Set the render layer mask of the player's lights to the appropriate team layer
        uint renderMask = teamIndex == 0 ? RenderingLayerMask.GetMask("Player1") : RenderingLayerMask.GetMask("Player2");
        Light[] lights = player.GetComponentsInChildren<Light>(includeInactive: true);

        foreach (Light light in lights)
        {
            UniversalAdditionalLightData additionalData = light.gameObject.GetComponent<UniversalAdditionalLightData>();
            if (additionalData.renderingLayers != RenderingLayerMask.GetMask("Default"))
            {
                additionalData.renderingLayers = renderMask;
                additionalData.shadowRenderingLayers = renderMask;

                light.renderingLayerMask = (int)renderMask;
            }
        }
    }

    private void ConfigCharacterRenderLayerMask(GameObject player, int teamIndex)
    {
        // Set the render layer mask of the player's renderers to the appropriate team layer
        uint layerMask = teamIndex == 0 ? RenderingLayerMask.GetMask("Player1") : RenderingLayerMask.GetMask("Player2");
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        SkinnedMeshRenderer[] skinnedMeshRenderers = player.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (Renderer renderer in renderers)
        {
            if(renderer.gameObject.CompareTag("ParrySphere")) continue; // Parry sphere blocks light if renderer is changed
            renderer.renderingLayerMask = layerMask;
        }

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
        {
            skinnedMeshRenderer.renderingLayerMask = layerMask;
        }
    }
}
