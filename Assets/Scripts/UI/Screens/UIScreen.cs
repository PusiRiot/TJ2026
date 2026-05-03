using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIScreen : MonoBehaviour
{
    [SerializeField] protected ScreenName screenName;
    [SerializeField] protected GameObject firstToNavigate;
    [SerializeField] TextMeshProUGUI exitText;
    string[] exitPlayerButton = new string[2];

    public string GetName()
    {
        return screenName.ToString();
    }

    virtual public void Hide()
    {
        gameObject.SetActive(false);
    }

    virtual public void Show()
    {
        if (screenName == ScreenName.Game)
        {
            MusicManager.Instance.PlayGamePlayMusic();

            if (!GameManager.Instance.GameInitializing)
                GameManager.Instance.UnpauseGame();
        }
        if (screenName == ScreenName.Pause)
        {
            MusicManager.Instance.PlayPauseMusic();
            GameManager.Instance.PauseGame();
        }

        EventSystem.current.SetSelectedGameObject(firstToNavigate);
        gameObject.SetActive(true);
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
