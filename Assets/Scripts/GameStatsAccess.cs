using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

/// <summary>
/// Game logic and state
/// </summary>
public class GameStatsAccess : MonoBehaviour
{
    [SerializeField] GameStats gameStats;


    #region Singleton implementation
    public static GameStatsAccess Instance { get; private set; }

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

        DontDestroyOnLoad(gameObject);
    }

    #endregion

    #region Get global game stats
    // Duration
    public float GetGameDuration() { return gameStats.GameDuration; }

    // Team colors and materials
    public Color GetTeamColor(int teamIndex){ return gameStats.TeamColor[teamIndex]; }
    public Material GetTeamEmissiveMaterial(int teamIndex){ return gameStats.TeamEmissiveMaterial[teamIndex]; }

    public Color GetDamageColor() { return gameStats.DamageColor; }
    public Color GetHealColor() { return gameStats.HealColor; }

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

    public float GetGroovyOutlineGlowUp() { return gameStats.GroovyOutlineGlowUp; }
    public float GetGroovyOutlineGlowDown() { return gameStats.GroovyOutlineGlowDown; }
    public int LightMeleeDamage(){ return gameStats.LightMeleeDamage; }
    public int HeavyMeleeDamage(){ return gameStats.HeavyMeleeDamage; }

    public float LightMeleeRange(){ return gameStats.LightMeleeRange; }

    public float HeavyMeleeDashDuration() { return gameStats.HeavyMeleeDashDuration; }

    public float HeavyMeleeDashSpeedIncrement() { return gameStats.HeavyMeleeDashSpeedIncrement; }

    public float HeavyMeleeLightOffDuration(){ return gameStats.HeavyMeleeLightOffDuration; }
    public float HeavyMeleeStunDuration(){ return gameStats.HeavyMeleeStunDuration; }
    public float SuccesfulParryLightOffDuration(){ return gameStats.SuccesfulParryLightOffDuration; }

    public float ParryDuration(){ return gameStats.ParryDuration; }

    public float SuccesfulParryStunDuration(){ return gameStats.SuccesfulParryStunDuration; }

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

    // Doors

    public int GetClosedDoorsOnAwake() { return gameStats.ClosedDoorsOnAwake; }
    public float GetBaseDoorRandom() { return gameStats.BaseDoorRandom; }
    public float GetBiasToCloseDoorRandom() { return gameStats.BiasToCloseDoorRandom; }
    public float GetBiasToOpenDoorRandom() { return gameStats.BiasToOpenDoorRandom; }
    public int GetMinDoorRandomTime() { return gameStats.MinDoorRandomTime; }
    public int GetMaxDoorRandomTime() { return gameStats.MaxDoorRandomTime; }
    #endregion
}
