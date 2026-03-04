using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract class representing a light source used by the player to interact with crystals.
/// <para>It periodically checks for crystals within its detection range. Any specific type of light (e.g., area, spotlight...) should inherit from this class and implement the DetectLightCollision.</para>
/// <remarks>Each light is associated with a player, that should be distinguised by a tag. 
/// That way, the light can give the crystal information about which team is lighting it up, so that it can update the score and choose the color accordingly. 
/// </remarks>
/// </summary>
public abstract class AbstractLight : MonoBehaviour
{
    private Light light;
    /// <summary>
    /// Team index of the player that owns this light, used to determine which team gets the score when lighting up a crystal and what color the light should be.
    /// </summary>
    protected int teamIndex;

    /// <summary>
    /// Only light up a crystal after the player has been shining on it for a certain amount of time, to prevent score changes from very quick flashes of light.
    /// </summary>
    protected float requiredHoldTime = 2f;

    private void Awake()
    {
        light = GetComponent<Light>();

        Player player = GetComponentInParent<Player>();
        if (player.gameObject.CompareTag("Player1"))
        {
            teamIndex = 0;
        }
        else if (player.gameObject.CompareTag("Player2"))
        {
            teamIndex = 1;
        }
        
        requiredHoldTime = GameManager.Instance.GetRequiredLightHoldTime();
    }
    private void Update()
    {
        DetectLightCollision();
    }

    /// <summary>
    /// Lights up a crystal if its on collision range (each light type should have a different way of lighting up the crystal, e.g. area light should light up all crystals in a certain radius, spot light should light up all crystals in a certain cone direction, etc.)
    /// </summary>
    protected virtual void DetectLightCollision()
    {
        throw new System.NotImplementedException("Implement on child object");
    }

    public void TurnOn()
    {
        light.enabled = true;
    }

    public void TurnOff()
    {
        light.enabled = false;
    }
}

