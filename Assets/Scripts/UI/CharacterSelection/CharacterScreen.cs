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
    [SerializeField] GameObject flashlightScreen;
    [SerializeField] GameObject skillScreen;

    private void Awake()
    {
        skillScreen.SetActive(false);
        flashlightScreen.SetActive(true);
    }

    public void ShowInfo(bool show)
    {
        skillScreen.SetActive(show);
        flashlightScreen.SetActive(!show);
    }
}