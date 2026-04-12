using UnityEngine;

/// <summary>
/// Script to generate players on runtime when game starts based on the player selection of GameGlobalSettings
/// </summary>
public class GeneratePlayers : MonoBehaviour
{
    [SerializeField] GameObject[] players = new GameObject[2];
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
                    ConfigCharacterLayer(instanciated, i);
                    ConfigCharacterRenderLayerMask(instanciated, i);
                    instanciated.SetActive(true);
                    break;
                }
            }
        }       
    }

    private void ConfigCharacterLayer(GameObject player, int teamIndex)
    {
        // Set the layer of the player and its children to the appropriate team layer
        int layer = teamIndex == 0 ? LayerMask.NameToLayer("Player1") : LayerMask.NameToLayer("Player2");
        player.layer = layer;
        foreach (Transform child in player.transform)
        {
            child.gameObject.layer = layer;
        }
    }

    private void ConfigCharacterRenderLayerMask(GameObject player, int teamIndex)
    {
        // Set the render layer mask of the player's renderers to the appropriate team layer
        uint layerMask = teamIndex == 0 ? RenderingLayerMask.GetMask("Player1") : RenderingLayerMask.GetMask("Player2");
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.renderingLayerMask = layerMask;
        }
    }
}
