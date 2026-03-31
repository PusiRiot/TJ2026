using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

/// <summary>
/// Game logic and state
/// </summary>
public class GameManager : Subject<GameEvent>
{
    #region Variables
    [SerializeField] GameStats gameStats;

    int[] teamScore = new int[2] { 0, 0 };

    bool suddenDeathEnabled = false;
    public bool SuddenDeathEnabled { get { return suddenDeathEnabled; } }

    int maxScore;
    #endregion

    #region Singleton implementation
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        UnpauseGame();
    }

    #endregion

    private void Start()
    {
        if (gameStats == null)
        {
            Debug.LogError("GameStats not assigned in GameManager!");
        }

        AddObserversOnScene();

        maxScore = FindObjectsByType<Crystal>(FindObjectsSortMode.None).Length;
    }

    #region Get global game stats
    // Duration
    public float GetGameDuration() { return gameStats.GameDuration; }

    // Team colors and materials
    public Color GetTeamColor(int teamIndex){ return gameStats.TeamColor[teamIndex]; }
    public Material GetTeamEmissiveMaterial(int teamIndex){ return gameStats.TeamEmissiveMaterial[teamIndex]; }

    // Crystal
    public float GetTotalReclaimCrystalPoints() { return gameStats.TotalReclaimCrystalPoints; }

    public float GetReclaimCrystalPointsPerSecond() { return gameStats.ReclaimCrystalPointsPerSecond; }

    public float GetCrystalInactiveResetPointsPerSecond() { return gameStats.CrystalInactiveResetPointsPerSecond; }
    public float GetCrystalTimeToInactiveReset() { return gameStats.CrystalTimeToInactiveReset; }
    public float GetCrystalCooldownDuration(){ return gameStats.CrystalCooldownDuration; }

    // Heal
    public float GetHealCadence() { return gameStats.HealCadence; }
    public int GetHealAmount() { return gameStats.HealAmount; }

    // Combat
    public int GetMaxLives() { return gameStats.MaxLives; }
    public int LightMeleeDamage(){ return gameStats.LightMeleeDamage; }
    public int HeavyMeleeDamage(){ return gameStats.HeavyMeleeDamage; }

    public float LightMeleeRange(){ return gameStats.LightMeleeRange; }

    public float HeavyMeleeDashDuration() { return gameStats.HeavyMeleeDashDuration; }

    public float HeavyMeleeDashSpeedIncrement() { return gameStats.HeavyMeleeDashSpeedIncrement; }

    public float HeavyMeleeLightOffDuration(){ return gameStats.HeavyMeleeLightOffDuration; }
    public float SuccesfulParryLightOffDuration(){ return gameStats.SuccesfulParryLightOffDuration; }

    public float ParryDuration(){ return gameStats.ParryDuration; }

    public float LightMeleeCooldownDuration(){ return gameStats.LightMeleeCooldownDuration; }
    public float HeavyMeleeCooldownDuration(){ return gameStats.HeavyMeleeCooldownDuration; }
    public float ParryCooldownDuration(){ return gameStats.ParryCooldownDuration; }
    public float DeathDuration(){ return gameStats.DeathDuration; }

    // Movement
    public float GetPlayerRotationSpeed(){ return gameStats.PlayerRotationSpeed; }
    public float GetDashDuration(){ return gameStats.DashDuration; }
    public float GetDashSpeedIncrement() { return gameStats.DashSpeedIncrement; }
    public float GetMaxStamina() { return gameStats.MaxStamina; }
    public float GetStaminaConsumption() { return gameStats.StaminaConsumption; }
    public float GetStaminaRegenRate() { return gameStats.StaminaRegenRate; }

    #endregion

    #region Pause Game
    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    public void UnpauseGame()
    {
        Time.timeScale = 1;
    }
    #endregion

    #region Score management

    public void ChangeScore(int teamIndex, int scoreChange)
    {
        teamScore[teamIndex] += scoreChange;
        Debug.Log($"Team {teamIndex} score changed! Current score: {teamScore[0]} - {teamScore[1]}");
        Notify(GameEvent.ScoreUpdate, teamScore);
        CheckWinCondition();
    }

    #endregion

    #region End game conditions management

    private void CheckWinCondition()
    {
        if (!suddenDeathEnabled)
        {
            if (teamScore[0] >= maxScore || teamScore[1] >= maxScore)
                Notify(GameEvent.GameEnd, teamScore);
        } else
        {
            if (teamScore[0] != teamScore[1])
                Notify(GameEvent.GameEnd, teamScore);
        }
    }

    public void TimerEnded()
    {
        if (teamScore[0] != teamScore[1])
        {
            Notify(GameEvent.GameEnd, teamScore);
        }
        else
        {
            Notify(GameEvent.SuddenDeath);
            suddenDeathEnabled = true;
        }
    }
    #endregion
}
