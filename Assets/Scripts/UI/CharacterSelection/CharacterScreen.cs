using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Character screen is the screen that shows the concept and info of a certain character.
/// <para>It is assigned to the character selection screen and shows/hides info when the player presses the info button.</para>
/// <para>It has a reference to the character so it can be searched.</para>
/// </summary>
public class CharacterScreen : MonoBehaviour
{
    [SerializeField] PlayerCharacter characterReference;
    [SerializeField] Image[] infoButtons = new Image[3];
    [SerializeField] GameObject[] infoTexts = new GameObject[3];
    [SerializeField] Color buttonNormalColor;
    [SerializeField] Color buttonSelectedColor;

    int currentButtonIdx;

    public PlayerCharacter CharacterReference => characterReference;
    [SerializeField] GameObject infoScreen;

    private void Awake()
    {
        infoScreen.SetActive(false);

        // -- select first button --
        currentButtonIdx = 0;

        // change colors of buttons
        infoButtons[0].color = buttonSelectedColor;
        infoButtons[0].gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1);
        infoButtons[1].color = buttonNormalColor;
        infoButtons[1].gameObject.transform.localScale = new Vector3(1, 1, 1);
        infoButtons[2].color = buttonNormalColor;
        infoButtons[2].gameObject.transform.localScale = new Vector3(1, 1, 1);

        // change text
        infoTexts[0].SetActive(true);
        infoTexts[1].SetActive(false);
        infoTexts[2].SetActive(false);
    }

    public void ShowInfo(bool show)
    {
        infoScreen.SetActive(show);
    }

    public void ChangeInfoButton(bool left)
    {
        int lastSelectedIdx = currentButtonIdx;

        if (left)
            currentButtonIdx = (currentButtonIdx - 1 + 3) % 3;
        else
            currentButtonIdx = (currentButtonIdx + 1 + 3) % 3;

        infoButtons[lastSelectedIdx].color = buttonNormalColor;
        infoButtons[lastSelectedIdx].gameObject.transform.localScale = new Vector3(1, 1, 1);
        infoButtons[currentButtonIdx].color = buttonSelectedColor;
        infoButtons[currentButtonIdx].gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1);

        infoTexts[lastSelectedIdx].SetActive(false);
        infoTexts[currentButtonIdx].SetActive(true);
    }
}