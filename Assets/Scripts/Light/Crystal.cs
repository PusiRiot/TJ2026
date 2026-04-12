using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Crystal class, attached to the crystal game objects. 
/// <para>It is responsible for managing its own emission and the call to GameManager to score changes when it is lit up by a player's light.</para>
/// </summary>
[RequireComponent(typeof(Light))]
public class Crystal : MonoBehaviour
{
    [Header("Crystal Light")]
    [SerializeField] float intensityWhileUnpicked = 0f; // Intensity of the crystal light when it's unlit
    [SerializeField] float intensityWhileCooling = 5f; // Intensity of the crystal light when it has just been lit and is in cooldown.
    [SerializeField] float capturingMinIntensity = 0.75f;
    [SerializeField] float capturingMaxIntensity = 5f;
    [SerializeField] float animateLightRate = 0.1f;

    [Header("Capturing Particles")]
    [SerializeField] List<ParticleSystem> capturingParticles;
    [SerializeField] float capturingParticlesMinSize = 0.25f;
    [SerializeField] float capturingParticlesMaxSize = 1.5f;
    [SerializeField] float animateParticlesRate = 0.1f;

    [Header("Captured Particles")]
    [SerializeField] List<ParticleSystem> capturedParticles;

    [Header("UI")]
    [SerializeField] UnityEngine.UI.Image captureProgressBar;
    [SerializeField] GameObject contestUI;


    private Light crystalLight;
    private bool isLit = false;
    // By default using generic color
    private int teamCaptured = 2;
    private bool cooldownActive = false;

    // Capture variables
    float reclaimPointsTotal;
    private float reclaimPointsCurrent = 0f;
    private List<Color> teamsColor = new List<Color>();

    Animator animator;

    bool animateParticles = false;
    bool animateLight = false;

    Material crystalMaterial;


    // Capture flags
    #region capture_flags
    // This can and might be done in a single List (memory optimization)
    private List<bool> teamsReclaiming           = new List<bool> { false, false };
    private List<bool> teamsReclaimingPrevFrame  = new List<bool> { false, false };
    private List<bool> teamsReclaimingFirstFrame = new List<bool> { false, false };

    float inactiveResetTime;
    private float inactiveCountdown = 0f;
    private float inactiveMinusPointsPerSecond;
    private UnityEvent inactiveActionPerFrame = new UnityEvent();


    // Reclaiming callbacks
    public  UnityEvent<int> reclaimingStartedCallback = new UnityEvent<int>();
    private UnityEvent<int> reclaimingUpdateCallback = new UnityEvent<int>();
    public  UnityEvent<int> reclaimingFinishedCallback = new UnityEvent<int>();

    // Contested callbacks
    public  UnityEvent contestedStartedCallback = new UnityEvent();
    private UnityEvent contestedUpdateCallback = new UnityEvent();
    public  UnityEvent contestedFinishedCallback = new UnityEvent();

    // Cooldown callbacks (when the crystal is not captured and lit)
    public UnityEvent<int> cooldownStartedCallback = new UnityEvent<int>();
    private UnityEvent cooldownUpdateCallback = new UnityEvent();
    public  UnityEvent cooldownFinishedCallback = new UnityEvent();
    #endregion


    private void Awake()
    {
        // Capturing started
        reclaimingStartedCallback.AddListener((team) => AkUnitySoundEngine.PostEvent("Play_Crystal_Capturing", gameObject));

        // Fully captured
        reclaimingFinishedCallback.AddListener((team) => AkUnitySoundEngine.PostEvent("Play_Captured_Crystal", gameObject));

        // Capture abandoned (inactive reset starts)
        inactiveActionPerFrame.AddListener(() => {
            if (inactiveCountdown == 0f) // first frame only
                AkUnitySoundEngine.PostEvent("Play_Crystal_Not_Capturing", gameObject);
        });

        inactiveResetTime = GameStatsAccess.Instance.GetCrystalTimeToInactiveReset();
        reclaimPointsTotal = GameStatsAccess.Instance.GetTotalReclaimCrystalPoints();
        inactiveMinusPointsPerSecond = GameStatsAccess.Instance.GetCrystalInactiveResetPointsPerSecond();
        crystalLight = GetComponent<Light>();
        crystalLight.intensity = intensityWhileUnpicked; // Set initial intensity to the "unpicked" value, which is the default state of the crystal

        teamsColor.Add(GameStatsAccess.Instance.GetTeamColor(0));   // Team 1 color
        teamsColor.Add(GameStatsAccess.Instance.GetTeamColor(1));   // Team 2 color
        teamsColor.Add(GameStatsAccess.Instance.GetTeamColor(2));   // Neutral color

        animator = GetComponent<Animator>();

        crystalLight.color = teamsColor[2]; // Set initial color to neutral
        inactiveActionPerFrame.AddListener(InactiveReset);
        inactiveActionPerFrame.AddListener(ShowInactiveResetFeedback);
        inactiveActionPerFrame.AddListener(() => animator.SetBool("capturing", false));
        inactiveActionPerFrame.AddListener(UpdateMatEmission);
        inactiveActionPerFrame.AddListener(() => UpdateMatColor(teamCaptured));

        // Get crystal material 
        crystalMaterial = GetComponentInChildren<MeshRenderer>().material;

        // Assing callbacks
        reclaimingStartedCallback.AddListener(ReclaimingStarted);
        reclaimingStartedCallback.AddListener((foo) => animator.SetBool("capturing", true));

        reclaimingUpdateCallback.AddListener(ShowCaptureFeedback);
        reclaimingUpdateCallback.AddListener(ReclaimingPerforming);
        reclaimingUpdateCallback.AddListener((foo) => UpdateMatEmission());

        reclaimingFinishedCallback.AddListener((foo) => reclaimPointsCurrent = 0);
        reclaimingFinishedCallback.AddListener(ReclaimingPerformed);
        reclaimingFinishedCallback.AddListener((foo) => { animator.SetTrigger("captured"); animator.SetBool("capturing", false);});
        reclaimingFinishedCallback.AddListener(UpdateMatColor);

        contestedStartedCallback.AddListener(() => animator.SetBool("contested", true));
        contestedStartedCallback.AddListener(ContestedStarted);

        contestedFinishedCallback.AddListener(() => animator.SetBool("contested", false));
        contestedFinishedCallback.AddListener(ContestedFinished);

        cooldownStartedCallback.AddListener(CooldownStarted);
        cooldownStartedCallback.AddListener((foo) => crystalMaterial.SetFloat("_Emission", 1));


        cooldownFinishedCallback.AddListener(CooldownFinished);
        cooldownFinishedCallback.AddListener(() => crystalMaterial.SetFloat("_Emission", 0));

    }

    private void LateUpdate()
    {
        ManageTeamsReclaim();
        ManageFinishActions();
        UpdateTeamsReclaimingList();
    }

    private void FixedUpdate()
    {
        // Animations to make smooth deactivations of light and particles
        if (animateParticles)
        {
            foreach (ParticleSystem p in capturingParticles)
            {
                var particlesMain = p.main;
                particlesMain.startSize = particlesMain.startSize.constant - Time.fixedDeltaTime * animateParticlesRate;

                if(particlesMain.startSize.constant <= 0)
                {
                    particlesMain.startSize = capturingParticlesMinSize;
                    p.gameObject.SetActive(false);
                    animateParticles = false;
                }
            }
        }

        if (animateLight)
        {
            crystalLight.intensity -= Time.fixedDeltaTime * animateLightRate;
            crystalMaterial.SetFloat("_Emission", crystalLight.intensity / intensityWhileCooling); // Update emission based on current intensity

            if (crystalLight.intensity <= 0)
            {
                crystalLight.intensity = 0;
                crystalMaterial.SetFloat("_Emission", 0);
                cooldownFinishedCallback.Invoke();
                animateLight = false;
            }
        }
    }

    private void ManageTeamsReclaim()
    {
        if (teamsReclaiming[0] && teamsReclaiming[1] && !cooldownActive)
        {
            if (!teamsReclaimingPrevFrame[0] || !teamsReclaimingPrevFrame[1])   // Check if this was the first frame of both teams reclaiming
                contestedStartedCallback.Invoke();

            // Both team are reclaiming
            contestedUpdateCallback.Invoke();
            inactiveCountdown = 0f;

        }
        else if (teamsReclaiming[0] == true && !cooldownActive && teamCaptured != 0)
        {
            if (!teamsReclaimingPrevFrame[0] || (teamsReclaimingPrevFrame[0] && teamsReclaimingPrevFrame[1])) // First time team 0 is reclaiming
                reclaimingStartedCallback.Invoke(0);
            else
            {
                inactiveCountdown = 0f;
                reclaimingUpdateCallback.Invoke(0);
            }
        }
        else if (teamsReclaiming[1] == true && !cooldownActive && teamCaptured != 1)
        {
            if (!teamsReclaimingPrevFrame[1] || (teamsReclaimingPrevFrame[0] && teamsReclaimingPrevFrame[1])) // First time team 1 is reclaiming
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

    private void ContestedStarted()
    {
        animateParticles = false;
        animateLight = false;

        //Crystal light
        float captureProgress = reclaimPointsCurrent / reclaimPointsTotal;
        crystalLight.intensity = capturingMinIntensity + captureProgress * (capturingMaxIntensity - capturingMinIntensity);

        // Activate both particles for feedback. Might change
        foreach (ParticleSystem p in capturingParticles)
        {
            var particlesMain = p.main;
            particlesMain.startSize = capturingParticlesMinSize + captureProgress * (capturingParticlesMaxSize - capturingParticlesMinSize);
            p.gameObject.SetActive(true);
        }

        // Activate contest UI element       
        contestUI.SetActive(true);
    }

    private void ContestedFinished()
    {
        animateLight = false;

        // Turn this teams particles off to allow the others to vanish
        if (teamCaptured != 2)
        {
            capturingParticles[teamCaptured].gameObject.SetActive(false);
        }

        if(reclaimPointsCurrent <= 0)
        {
            animateParticles = true;
            crystalLight.intensity = 0;
        } 

        // Lets see
        contestUI.SetActive(false);
    }

    public void ReclaimingPerforming(int teamIndex)
    {
        animateParticles = false;
        animateLight = false;

        float reclaimPointsPerSecond = GameStatsAccess.Instance.GetReclaimCrystalPointsPerSecond();
        float deltaTime = Time.deltaTime;
        float capturePointsGained = deltaTime * reclaimPointsPerSecond;
        reclaimPointsCurrent += capturePointsGained;
        captureProgressBar.gameObject.SetActive(true);
    }


    /// <summary>
    /// Player reclaimed crystal
    /// <para>Here code related to scoring and visual effects when just reclaimed</para>
    /// </summary>
    /// <param name="teamIndex"></param>
    public void ReclaimingPerformed(int teamIndex)
    {
        animateParticles = false;
        animateLight = false;

        if (cooldownActive) return; // Prevent multiple scoring while the crystal has just been lit

        // Deactivate particles
        foreach (ParticleSystem p in capturingParticles)
        {
            p.gameObject.SetActive(false);
        }

        //Deactivate progress bar
        captureProgressBar.gameObject.SetActive(false);

        capturedParticles[teamIndex].Play();

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
        cooldownStartedCallback.Invoke(teamIndex);
    }

    private void ReclaimingStarted(int teamIndex)
    {
        animateParticles = false;
        animateLight = false;

        captureProgressBar.gameObject.SetActive(true);
    }

    private void CooldownStarted(int teamIndex) {
        isLit = true;
        cooldownActive = true;
        crystalLight.color = GameStatsAccess.Instance.GetTeamColor(teamIndex);
        crystalLight.intensity = intensityWhileCooling; // You can adjust this value or make it a serialized field if you want different intensity for different crystals
        StartCoroutine(CooldownCountdown());
    }

    private void CooldownFinished()
    {
        cooldownActive = false;
    }

    IEnumerator CooldownCountdown()
    {
        // Make it so the cooldown ends exactly when the light turns off
        yield return new WaitForSeconds(GameStatsAccess.Instance.GetCrystalCooldownDuration() - (intensityWhileCooling / animateLightRate));
        animateLight = true;
    }

    private void ShowCaptureFeedback(int teamIndex)
    {
        //Crystal light
        float captureProgress = reclaimPointsCurrent / reclaimPointsTotal;
        crystalLight.intensity = capturingMinIntensity + captureProgress * (capturingMaxIntensity - capturingMinIntensity);
        crystalLight.color = teamsColor[teamIndex];

        // Capturing particles
        ParticleSystem teamParticles = capturingParticles[teamIndex];
        teamParticles.gameObject.SetActive(true);
        capturingParticles[(teamIndex + 1) % 2].gameObject.SetActive(false);
        var particlesMain = teamParticles.main;
        particlesMain.startSize = capturingParticlesMinSize + captureProgress * (capturingParticlesMaxSize - capturingParticlesMinSize);

        // Progress bar
        captureProgressBar.color = teamsColor[teamIndex];
        captureProgressBar.fillAmount = captureProgress;
    }

    private void ShowInactiveResetFeedback()
    {
        //Crystal light
        float captureProgress = reclaimPointsCurrent / reclaimPointsTotal;
        crystalLight.intensity = capturingMinIntensity + captureProgress * (capturingMaxIntensity - capturingMinIntensity);

        if (crystalLight.intensity <= capturingMinIntensity) crystalLight.intensity = intensityWhileUnpicked;

        // Progress bar
        captureProgressBar.fillAmount = captureProgress;
        if(captureProgressBar.fillAmount <= 0) captureProgressBar.gameObject.SetActive(false);
    }

    private void InactiveReset()
    {
        animateParticles = true;

        if (reclaimPointsCurrent <= 0) return;
        if (inactiveCountdown < inactiveResetTime)  // Check if a second has passed since the last time the crystal was being reclaimed
            return;

        reclaimPointsCurrent -= Time.deltaTime * inactiveMinusPointsPerSecond;
    }

    private void UpdateMatEmission()
    {
        float ratio = reclaimPointsCurrent / reclaimPointsTotal;
        crystalMaterial.SetFloat("_Emission", ratio);
    }

    private void UpdateMatColor(int teamIndex)
    {
        Color teamColor = teamsColor[teamIndex];
        crystalMaterial.SetColor("_ColorDark", teamColor);
    }

    public void ReclaimFlag(int teamIndex)
    {
        teamsReclaiming[teamIndex] = true;
    }

}

