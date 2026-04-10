using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Character selection button. It has methods to show animation when selected and a reference to the character it represents
/// </summary>
public class CharacterButton : MonoBehaviour
{
    [SerializeField] PlayerCharacter characterReference;
    public PlayerCharacter CharacterReference => characterReference;
    [SerializeField] Outline[] outline;
    bool[] playerHovering = new bool[2]; // 0 for P1, 1 for P2

    private void Awake()
    {
        for (int i = 0; i < 2; i++)
        {
            outline[i].enabled = false;
            outline[i].effectColor = GameStatsAccess.Instance.GetTeamColor(i);
        }
    }

    /// <summary>
    /// Select animation for player that selected and returns the button character
    /// </summary>
    public PlayerCharacter Select(int playerIndex)
    {
        Debug.Log("select character");
        playerHovering[playerIndex] = true;
        SelectionAnimation(playerIndex, true);
        return characterReference;

    }

    /// <summary>
    /// Stop animation of player that deselected
    /// </summary>
    /// <param name="eventData"></param>
    public void Deselect(int playerIndex)
    {
        playerHovering[playerIndex] = false;
        SelectionAnimation(playerIndex, false);
    }

    void SelectionAnimation(int playerIndex, bool selecting)
    {
        outline[playerIndex].enabled = selecting;
    }
}