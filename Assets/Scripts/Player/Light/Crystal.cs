using System.Collections;
using UnityEngine;

/// <summary>
/// Crystal class, attached to the crystal game objects. 
/// <para>It is responsible for managing its own emission and the call to GameManager to score changes when it is lit up by a player's light.</para>
/// </summary>
[RequireComponent(typeof(Light))]
public class Crystal : MonoBehaviour
{
    [SerializeField] float intensityWhileUnpicked = 0f; // Intensity of the crystal light when it's unlit and not picked.
    [SerializeField] float intensityWhilePicked = 0.5f; // Intensity of the crystal light when it's unlit but picked.
    [SerializeField] float intensityWhileCooling = 3f; // Intensity of the crystal light when it has just been lit and is in cooldown.
    private Light crystalLight;
    private bool isLit = false;
    private int lastTeamIndex = -1;
    private bool cooldownActive = false;
    private ParticleSystem particles;

    private void Awake()
    {
        particles = GetComponentInChildren<ParticleSystem>();
        crystalLight = GetComponent<Light>();
        crystalLight.intensity = intensityWhileUnpicked; // Set initial intensity to the "unpicked" value, which is the default state of the crystal
    }

    public void LightUp(int teamIndex)
    {
        if (cooldownActive) return; // Prevent multiple scoring while the crystal has just been lit

        if (teamIndex == lastTeamIndex) return; // Prevent scoring if the same team tries to light the crystal again

        if (isLit) // A different team is trying to light the crystal, add to their score and subtract from the previous team score
        {
            GameManager.Instance.ChangeScore(teamIndex, 1);
            GameManager.Instance.ChangeScore(lastTeamIndex, -1);
        }
        else // Crystal lit for the first time, just add to the team's score
        {
            GameManager.Instance.ChangeScore(teamIndex, 1);
        }

        TurnLightOn(teamIndex);
    }

    void TurnLightOn(int teamIndex)
    {
        particles.Play();
        lastTeamIndex = teamIndex;
        isLit = true;
        cooldownActive = true;
        crystalLight.color = GameManager.Instance.GetTeamColor(teamIndex);
        crystalLight.intensity = intensityWhileCooling; // You can adjust this value or make it a serialized field if you want different intensity for different crystals
        StartCoroutine(TurnLightOff());
    }

    IEnumerator TurnLightOff()
    {
        yield return new WaitForSeconds(GameManager.Instance.GetCrystalCooldownTime());
        crystalLight.intensity = intensityWhilePicked;
        cooldownActive = false;
    }
}
