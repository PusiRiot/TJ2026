using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Crystal class, attached to the crystal game objects. 
/// <para>It is responsible for managing its own emission and the call to GameManager to score changes when it is lit up by a player's light.</para>
/// </summary>
[RequireComponent(typeof(Light))]
public class Crystal : MonoBehaviour
{
    [SerializeField] float intensityWhileUnpicked = 0f; // Intensity of the crystal light when it's unlit and not picked.
    [SerializeField] float intensityWhilePicked = 0f; // Intensity of the crystal light when it's unlit but picked.
    [SerializeField] float intensityWhileCooling = 3f; // Intensity of the crystal light when it has just been lit and is in cooldown.

    private Light crystalLight;
    private bool isLit = false;
    private int lastTeamIndex = -1;
    private bool cooldownActive = false;
    private ParticleSystem particles;

    // Capture variables
    [SerializeField] const float capturePointsTotal = 30f;
    [SerializeField] float capturePointsPerSecond = 10f;
    [SerializeField] float coolCapturePointsPerSecond = 10f;
    private float capturePointsCurrent = 0f;
    private float secondsToCapture;

    private void Awake()
    {
        particles = GetComponentInChildren<ParticleSystem>();
        crystalLight = GetComponent<Light>();
        crystalLight.intensity = intensityWhileUnpicked; // Set initial intensity to the "unpicked" value, which is the default state of the crystal
    }

    /// <summary>
    /// Player just started to reclaim crystal
    /// <para>Here should be the code to slowly lit up crystal to give feedback to player</para>
    /// </summary>

    public void ReclaimingStarted()
    {
        Debug.Log("Reclaiming started");
    }

    // TODO: Fix all this mess about who calls who and when
    // TODO: Connect the lights color to the color of the team in the GameManager
    public bool ReclaimingPerforming(float totalCaptureTime)
    {
        Debug.Log("Reclaiming being performed");

        // Not efficient divisions but easier to understand, we can optimize later if needed
        capturePointsCurrent = totalCaptureTime * capturePointsPerSecond;
        crystalLight.intensity = (capturePointsCurrent / capturePointsTotal) * intensityWhileCooling;
        Debug.Log($"Timer: {totalCaptureTime:F2}s, points = {capturePointsCurrent:F2}");

        if (capturePointsCurrent >= capturePointsTotal) // Check if crystal is captured
        {
            capturePointsCurrent = capturePointsTotal; // Cap the capture points to the total
            return true; // Capture complete
        }
        return false;
    }


    /// <summary>
    /// Player reclaimed crystal
    /// <para>Here code related to scoring and visual effects when just reclaimed</para>
    /// </summary>
    /// <param name="teamIndex"></param>
    public void ReclaimingPerformed(int teamIndex)
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

    /// <summary>
    /// Player stopped illuminating crystal without reclaiming it
    /// <para>Here should be the code to lit down crystal again</para>
    /// </summary>
    public void ReclaimingCanceled()
    {

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
        yield return new WaitForSeconds(GameManager.Instance.GetCrystalCooldownDuration());
        crystalLight.intensity = intensityWhilePicked;
        cooldownActive = false;
    }

    /// <summary>
    /// Returns if the selected team can capture the crystal
    /// </summary>
    /// <param name="teamIndex"></param>
    /// <returns></returns>
    public bool CanCapture(int teamIndex)
    {
        return !cooldownActive && teamIndex != lastTeamIndex;
    }
}
