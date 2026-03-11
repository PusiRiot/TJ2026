using UnityEngine;

/// <summary>
/// This is where the current game global settings are stored (selected player characters, global score...)
/// </summary>

[CreateAssetMenu(fileName = "CurrentGameSettings", menuName = "Scriptable Objects/CurrentGameSettings")]
public class CurrentGameGlobalSettings : SingletonScriptableObject<CurrentGameGlobalSettings>
{
    public PlayerCharacter[] selectedPlayerCharacters = new PlayerCharacter[2];
    public int[] globalScore = new int[2];
}