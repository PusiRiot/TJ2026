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
    public float GameDuration = 120f; // if the time runs out, the team with the highest score wins. If it's a tie, the game goes to sudden death
    public Color[] TeamColor = new Color[2]; // team colors, to be used for the crystals and the player lights. The index of the color should correspond to the team index (e.g. teamColors[0] is the color for team 1)
    public Material[] TeamEmissiveMaterial = new Material[2]; // to be used for the crystal emissive material and dash
    public float ReclaimCrystalDuration = 3f; // Time the player needs to keep shining on a crystal for it to light up and change the score, to prevent score changes from very quick flashes of light.
    public float CrystalCooldownDuration = 5f; // Time a crystal needs to be unlit before it can be lit up again, to prevent score changes from very quick flashes of light and to add some strategy to the game.
    public float LightStunDuration = 1f;
    public float HeavyStunDuration = 3f;
    public float DashDuration = 0.2f;
    public float DashSpeedIncrement = 2f;
    public float MaxStamina = 100.0f;
    public float StaminaConsumption = 100.0f;
    public float StaminaRegenRate = 20.0f;
}
