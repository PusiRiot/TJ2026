using TMPro;
using UnityEngine;

public class UIScreenCharacterSelection : UIScreen
{
    CharacterSelection[] players;
    [SerializeField] TextMeshProUGUI exitText;
    string[] exitPlayerButton = new string[2];

    private void Awake()
    {
        players = GetComponentsInChildren<CharacterSelection>();

        foreach (var p in players)
        {
            p.PlayerReadyChanged.AddListener(OnPlayerReadyChanged);
        }
    }

    private void OnPlayerReadyChanged(bool _)
    {
        bool allReady = true;

        foreach (var p in players)
        {
            if (!p.PlayerReady)
            {
                allReady = false;
                break;
            }
        }

        if (allReady)
            UINavigationManager.Instance.LoadScene(SceneName.GameScene);
    }

    public void ChangeEscText(int playerIndex, string playerButton)
    {
        exitPlayerButton[playerIndex] = playerButton;
        if (exitPlayerButton[0] == exitPlayerButton[1])
            exitText.text = exitPlayerButton[0];
        else
            exitText.text = exitPlayerButton[0] + "/" + exitPlayerButton[1];

    }
}
