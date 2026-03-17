using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] List<ParticleSystem> capturingParticles;
    [SerializeField] float capturingParticlesMinSize = 0.25f;
    [SerializeField] float capturingParticlesMaxSize = 1.5f;

    private Light crystalLight;
    private bool isLit = false;
    private int teamCaptured = 2;
    private bool cooldownActive = false;

    // Capture variables
    [SerializeField] const float reclaimPointsTotal = 30f;
    private float reclaimPointsCurrent = 0f;
    private bool isBeingReclaimed = false;
    private bool isBeingContested = false;
    private List<Color> teamsColor = new List<Color>();
    private int lastTeamReclaiming = 2;


    // Capture flags
    private List<bool> teamsReclaiming = new List<bool> { false, false };
    [SerializeField] float inactiveResetTime = 1f;
    private float inactiveCountdown = 0f;
    private float inactiveMinusPointsPerSecond = 10f;
    private Action inactiveActionPerFrame;






    private void Awake()
    {
        crystalLight = GetComponent<Light>();
        crystalLight.intensity = intensityWhileUnpicked; // Set initial intensity to the "unpicked" value, which is the default state of the crystal

        teamsColor.Add(GameManager.Instance.GetTeamColor(0));   // Team 1 color
        teamsColor.Add(GameManager.Instance.GetTeamColor(1));   // Team 2 color
        teamsColor.Add(GameManager.Instance.GetTeamColor(2));   // Neutral color

        crystalLight.color = teamsColor[2]; // Set initial color to neutral
        inactiveActionPerFrame += InactiveReset;
    }


    /// <summary>
    /// Late Update to reset the teams reclaiming the crystal
    /// </summary>
    private void LateUpdate()
    {
        ManageTeamsReclaim();
    }


    /// <summary>
    /// Player just started to reclaim crystal
    /// <para>Here should be the code to slowly lit up crystal to give feedback to player</para>
    /// </summary>
    public void ReclaimingStarted(int teamIndex)
    {
        // Just set some parameters for crystal capture
        Debug.Log("Reclaiming started");
        capturingParticles[teamIndex].gameObject.SetActive(true);
    }

    // TODO: Connect the lights color to the color of the team in the GameManager


    /// <summary>
    /// Function called every frame while the player is reclaiming the crystal
    /// </summary>
    /// <param name="teamIndex"> </param>
    /// <param name="reclaimPointsPerSecond"></param>
    /// <returns></returns>
    public bool ReclaimingPerforming(int teamIndex, float reclaimPointsPerSecond)
    {
        if (CanStartReclaim(teamIndex))
        {
            ReclaimingStarted(teamIndex);
        }

        if (cooldownActive || teamIndex == teamCaptured) return false; // Prevent reclaiming while the crystal has just been lit


        float deltaTime = Time.deltaTime;
        float capturePointsGained = deltaTime * reclaimPointsPerSecond;
        reclaimPointsCurrent += capturePointsGained;

        ShowCaptureFeedback(teamCaptured, teamIndex);

        if (reclaimPointsCurrent >= reclaimPointsTotal) // Check if crystal is captured
        {
            reclaimPointsCurrent = 0; // Cap the capture points to the total
            ReclaimingPerformed(teamIndex);
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
        // Deactivate particles
        foreach(ParticleSystem p in capturingParticles)
        {
            p.gameObject.SetActive(false);
        }

        if (cooldownActive) return; // Prevent multiple scoring while the crystal has just been lit

        if (isLit) // A different team is trying to light the crystal, add to their score and subtract from the previous team score
        {
            GameManager.Instance.ChangeScore(teamIndex, 1);
            GameManager.Instance.ChangeScore(teamCaptured, -1);
        }
        else // Crystal lit for the first time, just add to the team's score
        {
            GameManager.Instance.ChangeScore(teamIndex, 1);
        }

        teamCaptured = teamIndex;
        TurnLightOn(teamIndex);
    }

    /// <summary>
    /// Player stopped illuminating crystal without reclaiming it
    /// <para>Here should be the code to lit down crystal again</para>
    /// </summary>
    public void ReclaimingCanceled()
    {
        // Deactivate particles
        foreach (ParticleSystem p in capturingParticles)
        {
            p.gameObject.SetActive(false);
        }
    }


    void TurnLightOn(int teamIndex)
    {
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

    private void ShowCaptureFeedback(int startingTeam, int endingTeam)
    {
        float captureProgress = reclaimPointsCurrent / reclaimPointsTotal;
        Color interpolatedColor = Color.Lerp(teamsColor[startingTeam], teamsColor[endingTeam], captureProgress);

        //Crystal
        crystalLight.intensity = captureProgress * intensityWhileCooling;
        crystalLight.color = interpolatedColor;

        //Particle
        var capturingParticlesMain = capturingParticles[endingTeam].main;
        capturingParticlesMain.startSize = capturingParticlesMinSize + (capturingParticlesMaxSize - capturingParticlesMinSize) * captureProgress;
    }

    /// <summary>
    /// Returns if the selected team can capture the crystal
    /// </summary>
    /// <param name="teamIndex"></param>
    /// <returns></returns>
    public bool CanStartReclaim(int teamIndex)
    {
        Debug.Log($"{!cooldownActive}, {reclaimPointsCurrent}");
        // return (!cooldownActive && reclaimPointsCurrent <= 1);
        // TODO: ADD FLAG TO KNOW WHEN IS THE FIRST FRAME RECLAIMING THE CRYSTAL
        return false;
    }

    private void ManageTeamsReclaim()
    {
        if (teamsReclaiming[0] == true && teamsReclaiming[1] == true)
        {
            // Both team are reclaiming
            inactiveCountdown = 0f;
            crystalLight.color = teamsColor[2]; // Set color to neutral when contested
        }
        else if (teamsReclaiming[0] == true)
        {
            // Team 1 is reclaiming
            inactiveCountdown = 0f;
            lastTeamReclaiming = 0;
            ReclaimingPerforming(0, GameManager.Instance.GetReclaimCrystalPointsPerSecond());

        }
        else if (teamsReclaiming[1] == true)
        {
            // Team 2 is reclaiming
            inactiveCountdown = 0f;
            lastTeamReclaiming = 1;
            ReclaimingPerforming(1, GameManager.Instance.GetReclaimCrystalPointsPerSecond());

        }
        else
        {
            // No team is reclaiming
            inactiveCountdown += Time.deltaTime;
            inactiveActionPerFrame.Invoke();
        }

        teamsReclaiming[0] = false;
        teamsReclaiming[1] = false;
    }

    private void InactiveReset()
    {
        if (reclaimPointsCurrent <= 0) {
            reclaimPointsCurrent = 0;
            return; 
        }
        if (inactiveCountdown < inactiveResetTime)
            return;

        reclaimPointsCurrent -= Time.deltaTime * inactiveMinusPointsPerSecond;
        ShowCaptureFeedback(lastTeamReclaiming, teamCaptured);

    }

    public void ReclaimFlag(int teamIndex)
    {
        teamsReclaiming[teamIndex] = true;
    }

}

