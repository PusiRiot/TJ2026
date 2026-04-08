using UnityEngine;

/// <summary>
/// Character screen is the screen that shows the concept and info of a certain character.
/// <para>It is assigned to the character selection screen and shows/hides info when the player presses the info button.</para>
/// <para>It has a reference to the character so it can be searched.</para>
/// </summary>
public class CharacterScreen : MonoBehaviour
{
    [SerializeField] PlayerCharacter characterReference;
    public PlayerCharacter CharacterReference => characterReference;
    [SerializeField] GameObject infoScreen;

    private void Awake()
    {
        infoScreen.SetActive(false);
    }

    public void ShowInfo(bool show)
    {
        infoScreen.SetActive(show);
    }
}