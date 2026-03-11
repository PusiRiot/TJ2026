using UnityEngine;

/// <summary>
/// Class made to be able to easily asocciate a UINavigationManager function with a button on a screen
/// </summary>
public class UIButton : MonoBehaviour 
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
        UINavigationManager.Instance.LoadScene(nextSceneName);
    }

    public void ReloadScene()
    {
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
}
