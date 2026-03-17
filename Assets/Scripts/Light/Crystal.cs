using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Crystal class, attached to the crystal game objects. 
/// <para>It is responsible for managing its own emission and the call to GameManager to score changes when it is lit up by a player's light.</para>
/// </summary>
[RequireComponent(typeof(Light))]
public class Crystal : MonoBehaviour
{
    [SerializeField] float intensityWhileUnpicked = 0f; // Intensity of the crystal light when it's unlit and not picked.
    [SerializeField] float intensityWhilePicked = 3f; // Intensity of the crystal light when it's unlit but picked.
    [SerializeField] float intensityWhileCooling = 5f; // Intensity of the crystal light when it has just been lit and is in cooldown.

    private Light crystalLight;
    private bool isLit = false;
    // By default using generic color
    private int teamCaptured = 2;
    private bool cooldownActive = false;
    private ParticleSystem particles;

    // Capture variables
    [SerializeField] const float reclaimPointsTotal = 30f;
    private float reclaimPointsCurrent = 0f;
    private List<Color> teamsColor = new List<Color>();


    // Capture flags
    // This can and might be done in a single List (memory optimization)
    private List<bool> teamsReclaiming           = new List<bool> { false, false };
    private List<bool> teamsReclaimingPrevFrame  = new List<bool> { false, false };
    private List<bool> teamsReclaimingFirstFrame = new List<bool> { false, false };

    [SerializeField] float inactiveResetTime = 1f;
    private float inactiveCountdown = 0f;
    private float inactiveMinusPointsPerSecond = 10f;
    private UnityEvent inactiveActionPerFrame = new UnityEvent();


    // Reclaiming callbacks
    public  UnityEvent<int> reclaimingStartedCallback = new UnityEvent<int>();
    private UnityEvent<int> reclaimingUpdateCallback = new UnityEvent<int>();
    public  UnityEvent<int> reclaimingFinishedCallback = new UnityEvent<int>();

    // Contested callbacks
    public UnityEvent contestedStartedCallback = new UnityEvent();
    private UnityEvent contestedUpdateCallback = new UnityEvent();
    public  UnityEvent contestedFinishedCallback = new UnityEvent();

    // Cooldown callbacks (when the crystal is not captured and lit)
    public UnityEvent cooldownStartedCallback = new UnityEvent();
    private UnityEvent cooldownUpdateCallback = new UnityEvent();
    public  UnityEvent cooldownFinishedCallback = new UnityEvent();

    private void Awake()
    {
        particles = GetComponentInChildren<ParticleSystem>();
        crystalLight = GetComponent<Light>();
        crystalLight.intensity = intensityWhileUnpicked; // Set initial intensity to the "unpicked" value, which is the default state of the crystal

        teamsColor.Add(GameManager.Instance.GetTeamColor(0));   // Team 1 color
        teamsColor.Add(GameManager.Instance.GetTeamColor(1));   // Team 2 color
        teamsColor.Add(GameManager.Instance.GetTeamColor(2));   // Neutral color

        crystalLight.color = teamsColor[2]; // Set initial color to neutral
        inactiveActionPerFrame.AddListener(InactiveReset);

        // Assing callbacks
        reclaimingUpdateCallback.AddListener((teamIndex) => ReclaimingPerforming(teamIndex)); 
        reclaimingUpdateCallback.AddListener((teamIndex) => IncreaseCaptureLight(teamIndex));

        reclaimingFinishedCallback.AddListener((foo) => reclaimPointsCurrent = 0);
        reclaimingFinishedCallback.AddListener(ReclaimingPerformed);
    }

    private void LateUpdate()
    {
        ManageTeamsReclaim();
        ManageFinishActions();
        UpdateTeamsReclaimingList();
    }

    private void ManageTeamsReclaim()
    {
        if (teamsReclaiming[0] && teamsReclaiming[1])
        {
            if (!teamsReclaimingPrevFrame[0] || !teamsReclaimingPrevFrame[1])   // Check if this was the first frame of both teams reclaiming
                contestedStartedCallback.Invoke();

            // Both team are reclaiming
            contestedUpdateCallback.Invoke();
            inactiveCountdown = 0f;
            crystalLight.color = teamsColor[2]; // Set color to neutral when contested

        }
        else if (teamsReclaiming[0] == true && !cooldownActive && teamCaptured != 0)
        {
            if (!teamsReclaimingPrevFrame[0]) // First time team 0 is reclaiming
                reclaimingStartedCallback.Invoke(0);
            else
            {
                inactiveCountdown = 0f;
                reclaimingUpdateCallback.Invoke(0);
            }

        }
        else if (teamsReclaiming[1] == true && !cooldownActive && teamCaptured != 1)
        {
            if(!teamsReclaimingPrevFrame[1]) // First time team 1 is reclaiming
                reclaimingStartedCallback.Invoke(1);
            else
            {
                // Team 2 is reclaiming
                inactiveCountdown = 0f;
                reclaimingUpdateCallback.Invoke(1);
            }

        }
        else if (reclaimPointsCurrent > 0) 
        {

            inactiveCountdown += Time.deltaTime;

            // No team is reclaiming
            if(inactiveCountdown > inactiveResetTime)
            {
                if((inactiveCountdown - Time.deltaTime) <= inactiveResetTime) // Check if this is the first frame of being inactive
                    cooldownStartedCallback.Invoke();
            }

            inactiveActionPerFrame.Invoke();
        }
    }

    private void ManageFinishActions()
    {
        if (teamsReclaimingPrevFrame[0] && teamsReclaimingPrevFrame[1])
        {
            if (!teamsReclaiming[0] || !teamsReclaiming[1])    // Check if one of the teams stopped reclaiming
                contestedFinishedCallback.Invoke();
        }

        if(reclaimPointsCurrent >= reclaimPointsTotal)
        {
            if(teamsReclaiming[0])
                reclaimingFinishedCallback.Invoke(0);
            else
                reclaimingFinishedCallback.Invoke(1);
        }

        // TODO: Add cooldown finished callback invoke 
    }

    private void UpdateTeamsReclaimingList()
    {
        teamsReclaimingPrevFrame[0] = teamsReclaiming[0];
        teamsReclaimingPrevFrame[1] = teamsReclaiming[1];

        if (teamsReclaiming[0] && !teamsReclaimingPrevFrame[0])
        {
            teamsReclaimingFirstFrame[0] = true;
        }
        else
        {
            teamsReclaimingFirstFrame[0] = false;
        }

        if (teamsReclaiming[1] && !teamsReclaimingPrevFrame[1])
        {
            teamsReclaimingFirstFrame[1] = true;
        }
        else
        {
            teamsReclaimingFirstFrame[1] = false;
        }

        teamsReclaiming[0] = false;
        teamsReclaiming[1] = false;
    }

    public void ReclaimingPerforming(int teamIndex)
    {
        float reclaimPointsPerSecond = GameManager.Instance.GetReclaimCrystalPointsPerSecond();
        float deltaTime = Time.deltaTime;
        float capturePointsGained = deltaTime * reclaimPointsPerSecond;
        reclaimPointsCurrent += capturePointsGained;
    }


    /// <summary>
    /// Player reclaimed crystal
    /// <para>Here code related to scoring and visual effects when just reclaimed</para>
    /// </summary>
    /// <param name="teamIndex"></param>
    public void ReclaimingPerformed(int teamIndex)
    {
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
        crystalLight.intensity = intensityWhileUnpicked;
        cooldownActive = false;
    }

    private void IncreaseCaptureLight(int teamIndex)
    {
        float captureProgress = reclaimPointsCurrent / reclaimPointsTotal;
        crystalLight.intensity = captureProgress * intensityWhilePicked;
        Color lastColor = teamsColor[2];
        if (isLit)
        {
            lastColor = teamsColor[teamCaptured];
        }
        InterpolateBetweenColors(lastColor, teamsColor[teamIndex], captureProgress);
    }

    private void InterpolateBetweenColors(Color baseColor, Color destintyColor, float p)
    {
        Color colorLerped = Color.Lerp(baseColor, destintyColor, p);
        crystalLight.color = colorLerped;
    }

    private void InactiveReset()
    {
        if (reclaimPointsCurrent <= 0) return;
        if (inactiveCountdown < inactiveResetTime)  // Check if a second has passed since the last time the crystal was being reclaimed
            return;

        // Adjust intensity to the current amount of points
        reclaimPointsCurrent = Time.deltaTime * inactiveMinusPointsPerSecond;
        crystalLight.intensity -= reclaimPointsCurrent / reclaimPointsTotal * intensityWhilePicked;

    }

    public void ReclaimFlag(int teamIndex)
    {
        teamsReclaiming[teamIndex] = true;
    }

}

