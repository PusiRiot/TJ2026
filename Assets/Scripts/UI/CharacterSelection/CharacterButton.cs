using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Character selection button, when pressed, assigns the character to the player that pressed it and updates the character screen with the new character info.
/// </summary>
public class CharacterButton : MonoBehaviour
{
    [SerializeField] PlayerCharacter characterReference;
    public PlayerCharacter CharacterReference => characterReference;
    [SerializeField] Outline[] outline;
    [SerializeField] GameStats gameStats;
    bool[] playerHovering = new bool[2]; // 0 for P1, 1 for P2

    private void Awake()
    {
        for (int i = 0; i < 2; i++)
        {
            outline[i].enabled = false;
            outline[i].effectColor = gameStats.TeamColor[i];
        }
    }

    /// <summary>
    /// Assign the character to the player that pressed the button and update the player character screen with the new character info.
    /// </summary>
    public PlayerCharacter Select(int playerIndex)
    {
        Debug.Log("select character");
        playerHovering[playerIndex] = true;
        SelectionAnimation(playerIndex, true);
        return characterReference;

    }

    /// <summary>
    /// Change the outline of the button when the player is hovering it to show which character they are currently hovering on.
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