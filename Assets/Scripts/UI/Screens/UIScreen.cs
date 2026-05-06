using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIScreen : MonoBehaviour
{
    [SerializeField] protected ScreenName screenName;
    [SerializeField] protected GameObject firstToNavigate;
    GameObject lastToNavigate;
    UIBackButton backBtn;

    virtual protected void Awake()
    {
        backBtn = GetComponentInChildren<UIBackButton>(true);

    }
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
            AkUnitySoundEngine.SetRTPCValue("Music_LowPassFilter", 0f, null, 1000);
            AkUnitySoundEngine.SetRTPCValue("Music_Speed", 50f, null, 500);
            if (!GameManager.Instance.GameInitializing)
                GameManager.Instance.UnpauseGame();
        }
        if (screenName == ScreenName.Pause)
        {
            AkUnitySoundEngine.SetRTPCValue("Music_LowPassFilter", 60f, null, 500);
            AkUnitySoundEngine.SetRTPCValue("Music_Speed", 25f, null, 500);
            //MusicManager.Instance.PlayPauseMusic();
            GameManager.Instance.PauseGame();
        }


        Debug.Log("firstToNavigate " + firstToNavigate + " lastToNavigate: " + lastToNavigate);
        if (lastToNavigate == null)
            EventSystem.current.SetSelectedGameObject(firstToNavigate);
        else
            EventSystem.current.SetSelectedGameObject(lastToNavigate);

        gameObject.SetActive(true);
    }

    /// <summary>
    /// This is used to control what the last used button on a screen was before changing screens, so that it can go back to it when shown again
    /// </summary>
    /// <param name="lastToNavigate"></param>
    public void UpdateLastToNavigate(GameObject lastToNavigate)
    {
        if (firstToNavigate != null) // if the screen has navigation
            this.lastToNavigate = lastToNavigate;
    }

    public bool HasBackButton()
    {
        return backBtn != null;
    }

    public void ChangeBackButtonText(string text)
    {
        if (backBtn != null)
            backBtn.ChangeText(text);
    }
}
