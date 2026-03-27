using UnityEngine;

/// <summary>
/// Global place to set the game rules and stats. 
/// <para>Most useful if multiple GameStats settings exist on the game. It should be given on the Inspector of the Game Manager (or Level Manager if we have one)</para>
/// <para>This can: be easily accessed by the game designer from the editor, be used for multiple scenes, be modified on runtime by other scripts</para>
/// <para>NOTE: Attributes subject to change, Im just building a skeleton of what could the use of this Scriptable Object be</para>
/// </summary>

[CreateAssetMenu(fileName = "GameStats", menuName = "Scriptable Objects/GameStats")]
public class GameStats : ScriptableObject
{
    [Header ("Duration")]
    public float GameDuration = 120f; // if the time runs out, the team with the highest score wins. If it's a tie, the game goes to sudden death

    [Header ("Team colors and materials")]
    public Color[] TeamColor = new Color[2]; // team colors, to be used for the crystals and the player lights. The index of the color should correspond to the team index (e.g. teamColors[0] is the color for team 1)
    public Material[] TeamEmissiveMaterial = new Material[2]; // to be used for the crystal emissive material and dash

    [Header ("Crystal")]
    public float ReclaimCrystalPointsPerSecond = 10f; // Number of points the player reclaims at the crystal per second
    public float CrystalCooldownDuration = 5f; // Time a crystal needs to be unlit before it can be lit up again, to prevent score changes from very quick flashes of light and to add some strategy to the game.


    [Header("Combat")]
    public int MaxLives = 6;

    public int LightMeleeDamage = 1;
    public int HeavyMeleeDamage = 3;

    public float HeavyMeleeDashDuration = 0.2f;
    public float HeavyMeleeDashSpeedIncrement = 1f;

    public float HeavyMeleeLightOffDuration = 3;
    public float SuccesfulParryLightOffDuration = 3;

    public float DeathDuration = 7;

    public float ParryDuration = 1f;

    public float LightMeleeCooldownDuration = 1f;
    public float HeavyMeleeCooldownDuration = 1f;
    public float ParryCooldownDuration = 3f;

    [Header ("Movement")]
    public float DashDuration = 0.2f;
    public float DashSpeedIncrement = 2f;
    public float MaxStamina = 100.0f;
    public float StaminaConsumption = 100.0f;
    public float StaminaRegenRate = 20.0f;

}
