using UnityEngine;

/// <summary>
/// This is where the current game global settings are stored (selected player characters, global score...)
/// </summary>
public class GameGlobalSettings : MonoBehaviour
{
    private PlayerCharacter[] selectedPlayerCharacters = new PlayerCharacter[2];
    public static GameGlobalSettings Instance { get; private set; }

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

        // DEFAULT SETTINGS
        selectedPlayerCharacters[0] = PlayerCharacter.DrHives;
        selectedPlayerCharacters[1] = PlayerCharacter.Peggy;
    }

    public PlayerCharacter GetPlayerCharacter(int playerIndex)
    {
        return selectedPlayerCharacters[playerIndex];
    }

    public void SetPlayerCharacter(int playerIndex, PlayerCharacter character)
    {
        selectedPlayerCharacters[playerIndex] = character;
    }
}