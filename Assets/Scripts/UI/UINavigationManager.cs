using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class is responsible for managing all the main button clicks events under the main canvas of a scene that have to do with changing between screens and scenes, etc.
/// <para> A screen is defined as a child object on a canvas that has all the information that needs to be displayed at a certain moment (and has the UIScreen class). This manager has a dictionary to all the screens under the canvas, and is be able to switch between them by their name </para>
/// <para> To switch between screens call SwitchScreen</para>
/// <para> To switch between scenes call LoadScene</para>
/// <para> To reload game call ReloadScene</para>
/// </summary>
public class UINavigationManager : MonoBehaviour
{

    [SerializeField] private ScreenName _firstShownScreenName; // the screen that will first be visible on the canvas when the scene starts
    private IDictionary<string, UIScreen> _screens;
    private UIScreen _currentScreen;
    public UIScreen CurrentScreen {  get { return _currentScreen; } }
    private UIScreen _lastScreen;

    #region Singleton implementation
    public static UINavigationManager Instance { get; private set; }

    /// <summary>
    /// Awake does the singleton implementation
    /// <para>It also enables all screens on Awake so they can be initialized</para>
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        UIScreen[] screensUnderManager = GetComponentsInChildren<UIScreen>(true);

        foreach (UIScreen screen in screensUnderManager)
        {
            screen.Show();
        }
    }

    #endregion


    /// <summary>
    /// Hides all screens on Start and stores them in a dictionary for easy access, then shows the first screen defined on the inspector
    /// </summary>

    private void Start() // some things need to go before
    {
        // store all the screens (even the inactive ones) in a dictionary
        _screens = new Dictionary<string, UIScreen>();
        UIScreen[] screensUnderManager = GetComponentsInChildren<UIScreen>(true);

        foreach (UIScreen screen in screensUnderManager)
        {
            screen.Hide();
            _screens.Add(screen.GetName(), screen);
        }

        ShowScreen(_firstShownScreenName); // show the first screen
    }

    #region Screen navigation related methods

    /// <summary>
    /// This method finds a screen under a canvas by name and shows it on top of others. 
    /// <para>By default it hides the previous screen, but you can set the boolean to false (useful for popups)</para>
    /// </summary>
    /// <param name="screenName"></param>
    /// <param name="hidePreviousScreen"></param>
    public void ShowScreen(ScreenName screenName, bool hidePreviousScreen = true)
    {
        // try to find the screen by its name on the dictionary
        UIScreen screenToSwitch;
        bool foundScreen = _screens.TryGetValue(screenName.ToString(), out screenToSwitch);

        if (!foundScreen)
        {
            return; // if the screen wasn't found, return without switching
        }

        if (_currentScreen != null && hidePreviousScreen) // if there's a screen showing at the moment of the change hide it and store it as the previous screen
        {
            _currentScreen?.Hide();
        }

        _lastScreen = _currentScreen;
        _currentScreen = screenToSwitch;
        _currentScreen.Show();
    }

    /// <summary>
    /// Hide current screen. Useful for popups
    /// </summary>
    public void HideCurrentScreen()
    {
        _currentScreen?.Hide();
        _currentScreen = _lastScreen;
    }
    #endregion

    #region Scene change methods
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadScene(SceneName nextSceneName)
    {
        SceneManager.LoadScene(nextSceneName.ToString());
    }
    #endregion
}
