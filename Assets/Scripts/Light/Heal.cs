using Microsoft.Unity.VisualStudio.Editor;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.UIElements;

/// <summary>
/// Heal class, attached to the heal game objects. 
/// </summary>
public class Heal : MonoBehaviour
{
    [SerializeField] List<ParticleSystem> teamParticles;
    [SerializeField] List<ParticleSystem> teamPulseParticles;
    [SerializeField] ParticleSystem greenParticles;
    [SerializeField] float animateParticlesRate = 0.1f;
    [SerializeField] GameObject contestUI;

    Animator animator;

    bool animateParticles = false;

    float teamParticlesSize;

    private float healCadence;
    private int healAmount;

    // Capture flags
    // This can and might be done in a single List (memory optimization)
    private List<bool> teamsReclaiming           = new List<bool> { false, false };
    private List<bool> teamsReclaimingPrevFrame  = new List<bool> { false, false };

    // Reclaiming callbacks
    public  UnityEvent<int> reclaimingStartedCallback = new UnityEvent<int>();
    private UnityEvent<int> reclaimingUpdateCallback = new UnityEvent<int>();
    public  UnityEvent reclaimingFinishedCallback = new UnityEvent();

    // Contested callbacks
    public UnityEvent contestedStartedCallback = new UnityEvent();
    private UnityEvent contestedUpdateCallback = new UnityEvent();
    public  UnityEvent contestedFinishedCallback = new UnityEvent();

    private void Awake()
    {
        healCadence = GameManager.Instance.GetHealCadence();
        healAmount = GameManager.Instance.GetHealAmount();

        animator = GetComponent<Animator>();

        teamParticlesSize = greenParticles.main.startSize.constant;

        // Assing callbacks
        reclaimingStartedCallback.AddListener(ReclaimingStarted);
        reclaimingStartedCallback.AddListener((foo) => animator.SetBool("capturing", true));

        reclaimingUpdateCallback.AddListener(ReclaimingPerforming);

        reclaimingFinishedCallback.AddListener(ReclaimingFinished);
        reclaimingFinishedCallback.AddListener(() => animator.SetBool("capturing", false));  

        contestedStartedCallback.AddListener(() => animator.SetBool("contested", true));
        contestedStartedCallback.AddListener(ContestedStarted);

        contestedFinishedCallback.AddListener(() => animator.SetBool("contested", false));
        contestedFinishedCallback.AddListener(ContestedFinished);
    }

    private void LateUpdate()
    {
        ManageTeamsReclaim();
        ManageFinishActions();
        UpdateTeamsReclaimingList();
    }

    private void FixedUpdate()
    {
        // Animations to make smooth deactivations of particles
        if (animateParticles)
        {
            foreach (ParticleSystem p in teamParticles)
            {
                var particlesMain = p.main;
                particlesMain.startSize = particlesMain.startSize.constant - Time.fixedDeltaTime * animateParticlesRate;

                if(particlesMain.startSize.constant <= 0)
                {
                    p.gameObject.SetActive(false);
                }
            }

            var particlesMainGreen = greenParticles.main;
            particlesMainGreen.startSize = particlesMainGreen.startSize.constant - Time.fixedDeltaTime * animateParticlesRate;
            
            if (particlesMainGreen.startSize.constant <= 0)
            {
                greenParticles.gameObject.SetActive(false);
                animateParticles = false;
            }
        }
    }

    private void ManageTeamsReclaim()
    {
        if (teamsReclaiming[0] && teamsReclaiming[1])
        {
            if (!teamsReclaimingPrevFrame[0] || !teamsReclaimingPrevFrame[1])   // Check if this was the first frame of both teams reclaiming
                contestedStartedCallback.Invoke();

            // Both team are reclaiming
            contestedUpdateCallback.Invoke();

        }
        else if (teamsReclaiming[0] == true)
        {
            if (!teamsReclaimingPrevFrame[0] || (teamsReclaimingPrevFrame[0] && teamsReclaimingPrevFrame[1]))
            {
                // First time team 0 is reclaiming
                Debug.Log(teamsReclaimingPrevFrame[0] + " " + teamsReclaimingPrevFrame[1]);
                reclaimingStartedCallback.Invoke(0);
            }
            else
            {
                reclaimingUpdateCallback.Invoke(0);
            }
        }
        else if (teamsReclaiming[1] == true)
        {
            if (!teamsReclaimingPrevFrame[1] || (teamsReclaimingPrevFrame[0] && teamsReclaimingPrevFrame[1]))
            {
                // First time team 0 is reclaiming
                Debug.Log(teamsReclaimingPrevFrame[0] + " " + teamsReclaimingPrevFrame[1]);
                reclaimingStartedCallback.Invoke(1);
            }
            else
            {
                // Team 2 is reclaiming
                reclaimingUpdateCallback.Invoke(1);
            }

        }
    }

    private void ManageFinishActions()
    {
        if (teamsReclaimingPrevFrame[0] && teamsReclaimingPrevFrame[1])
        {
            if (!teamsReclaiming[0] || !teamsReclaiming[1])    // Check if one of the teams stopped reclaiming
                contestedFinishedCallback.Invoke();
        }

        // First frame of inactivity
        else if (teamsReclaimingPrevFrame[0] || teamsReclaimingPrevFrame[1])
        {
            if(!teamsReclaiming[0] && !teamsReclaiming[1])
                reclaimingFinishedCallback.Invoke();
        }
    }

    private void UpdateTeamsReclaimingList()
    {
        teamsReclaimingPrevFrame[0] = teamsReclaiming[0];
        teamsReclaimingPrevFrame[1] = teamsReclaiming[1];

        teamsReclaiming[0] = false;
        teamsReclaiming[1] = false;
    }

    private void ContestedStarted()
    {
        StopAllCoroutines();
        animateParticles = false;

        // Activate both particles for feedback. Might change
        foreach (ParticleSystem p in teamParticles)
        {
            var particlesMain = p.main;
            particlesMain.startSize = teamParticlesSize;
            p.gameObject.SetActive(true);
        }

        // Activate contest UI element       
        contestUI.SetActive(true);
    }

    private void ContestedFinished()
    {
        animateParticles = true;

        // Lets see
        contestUI.SetActive(false);
    }

    public void ReclaimingPerforming(int teamIndex)
    {
        var teamParticlesMain = teamParticles[teamIndex].main;
        teamParticlesMain.startSize = teamParticlesSize;
        var greenParticlesMain = greenParticles.main;
        greenParticlesMain.startSize = teamParticlesSize;
    }


    public void ReclaimingFinished()
    {
        animateParticles = true;
        StopAllCoroutines();
    }

    private void ReclaimingStarted(int teamIndex)
    {
        StopAllCoroutines();

        teamParticles[teamIndex].gameObject.SetActive(true);
        var teamParticlesMain = teamParticles[teamIndex].main;
        teamParticlesMain.startSize = teamParticlesSize;

        greenParticles.gameObject.SetActive(true);
        var greenParticlesMain = greenParticles.main;
        greenParticlesMain.startSize = teamParticlesSize;

        StartCoroutine(Pulse(teamIndex));

        Debug.Log("Team " + teamIndex + " started reclaiming heal");
    }

    public void ReclaimFlag(int teamIndex)
    {
        teamsReclaiming[teamIndex] = true;
    }

    private IEnumerator Pulse(int teamIndex)
    {
        while(true)
        {
            yield return new WaitForSeconds(healCadence);
            animator.SetTrigger("pulse");
            teamPulseParticles[teamIndex].Play();
            // Heal player
        }
    }

}

