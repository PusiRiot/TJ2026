using System.Collections;
using UnityEngine;

/// <summary>
/// Abstract class representing a light source used by the player to interact with crystals.
/// <para>It periodically checks for crystals within its detection range. Any specific type of light (e.g., area, spotlight...) should inherit from this class and implement the DetectLightCollision.</para>
/// <remarks>Each light is associated with a player, that should be distinguised by a tag. 
/// That way, the light can give the crystal information about which team is lighting it up, so that it can update the score and choose the color accordingly. 
/// </remarks>
/// </summary>
public class PlayerHighlight : MonoBehaviour, IObserver<GameUIAnimEvents>
{
    /// <summary>
    /// Team index of the player that owns this light, used to determine which team gets the score when lighting up a crystal and what color the light should be.
    /// </summary>
    protected int teamIndex;

    /// <summary>
    /// First light is the top one highlighting the player, the rest are ordered clockwise
    /// </summary>
    public Light[] lights;

    private float[] lightsBaseIntensity;

    private void Start()
    {
        Player player = GetComponentInParent<Player>();
        if (player.gameObject.CompareTag("Player1"))
        {
            teamIndex = 0;
        }
        else if (player.gameObject.CompareTag("Player2"))
        {
            teamIndex = 1;
        }

        lightsBaseIntensity = new float[lights.Length];
        for (int i = 0; i < lights.Length; i++)
        {
            lightsBaseIntensity[i] = lights[i].intensity;
            lights[i].intensity = 0;
        }

        lights[0].color = GameStatsAccess.Instance.GetTeamColor(teamIndex);
    }

    IEnumerator TurnOnLights()
    {
        float duration = 0.2f;
        while (true)
        {
            float t = Time.unscaledDeltaTime / duration;

            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].intensity = Mathf.Lerp(lights[i].intensity, lightsBaseIntensity[i], t);
            }
            yield return null;
        }
    }

    #region Observer
    public void OnNotify(GameUIAnimEvents evt, object data = null)
    {
        if (evt == GameUIAnimEvents.PlayersLightOn)
            StartCoroutine(TurnOnLights());
    }
    #endregion
}

