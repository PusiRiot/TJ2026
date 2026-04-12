using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Class made to be able to easily asocciate a UINavigationManager function with a button on a screen
/// </summary>
public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    /// <summary>
    /// If this button is used to switch to another screen give the correspondant value to nextScreenName
    /// </summary>
    [SerializeField] ScreenName nextScreenName;
    /// <summary>
    /// If this button is used to switch to another scene give the correspondant value to nextSceneName
    /// </summary>
    [SerializeField] SceneName nextSceneName;

    public void ShowNextScreen(bool hideCurrentScreen = true)
    {
        UINavigationManager.Instance.ShowScreen(nextScreenName, hideCurrentScreen);
    }

    public void LoadNextScene()
    {
        if (nextSceneName == SceneName.MainMenuScene)
            MusicManager.Instance.PlayTitleMusic();
        else if (nextSceneName == SceneName.GameScene)
            MusicManager.Instance.PlayGamePlayMusic();

        UINavigationManager.Instance.LoadScene(nextSceneName);
    }

    public void ReloadScene()
    {
        MusicManager.Instance.PlayGamePlayMusic();
        UINavigationManager.Instance.ReloadScene();
    }

    public void HideCurrentScreen()
    {
        UINavigationManager.Instance.HideCurrentScreen();
    }

    public void GoToUrl(string url)
    {
        Application.OpenURL(url);
    }

    public void ExitApp()
    {
        Application.Quit();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        AkUnitySoundEngine.PostEvent("Select_UI", gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        AkUnitySoundEngine.PostEvent("Click_UI", gameObject);
    }

}
