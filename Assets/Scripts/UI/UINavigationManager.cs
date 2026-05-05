using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
    [SerializeField] InputActionAsset inputAction;
    [SerializeField] private ScreenName _firstShownScreenName; // the screen that will first be visible on the canvas when the scene starts
    private IDictionary<string, UIScreen> _screens;
    private UIScreen _currentScreen;
    public UIScreen CurrentScreen {  get { return _currentScreen; } }
    private Stack<UIScreen> _screenStack = new();

    // Back button
    string[] backBtnBinding = new string[3]; // 0 for keyboard, 1 for gamepad, 2 for both
    int currentBinding;

    // Handle navigation in both mouse and keyboard/Gamepad
    GameObject lastUsedButton;
    bool isUsingMouse = false;

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

        InitializeDeviceBindings();
        AssignDevices();

        UIScreen[] screensUnderManager = GetComponentsInChildren<UIScreen>(true);

        foreach (UIScreen screen in screensUnderManager)
        {
            screen.Show();
        }
    }

    #endregion

    #region MonoBehaviour
    void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
        InputSystem.onActionChange += OnActionChange;
        inputAction.actionMaps[0].Enable();
        inputAction.actionMaps[0].FindAction("Esc").performed += OnEsc;
    }

    void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
        InputSystem.onActionChange -= OnActionChange;
        inputAction.actionMaps[0].FindAction("Esc").performed -= OnEsc;
    }

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

    private void Update()
    {
        HandleControllerMouseNavigation();
    }
    #endregion

    #region Screen navigation related methods

    /// <summary>
    /// This method finds a screen under a canvas by name and shows it on top of others. 
    /// <para>By default it hides the previous screen, but you can set the boolean to false (useful for popups)</para>
    /// </summary>
    /// <param name="screenName"></param>
    /// <param name="hidePreviousScreen"></param>
    public void ShowScreen(ScreenName screenName, bool hidePreviousScreen = true)
    {
        // try to find the new screen by its name on the dictionary
        UIScreen screenToSwitch;
        bool foundScreen = _screens.TryGetValue(screenName.ToString(), out screenToSwitch);

        if (!foundScreen)
        {
            return; // if the screen wasn't found, return without switching
        }

        HandleScreenChange(screenToSwitch, hidePreviousScreen, true);
    }

    void HandleScreenChange(UIScreen screenToSwitch, bool hidePreviousScreen, bool pushPreviousToStack)
    {
        if (_currentScreen != null)
        {
            // update the last navigated item on current screen to select it as first when going back to that screen
            _currentScreen.UpdateLastToNavigate(lastUsedButton);

            // hide current screen
            if (hidePreviousScreen)
                _currentScreen.Hide();

            // stack screen to return to it with BackButton
            if (pushPreviousToStack)
                _screenStack.Push(_currentScreen);
        }

        // change screen to new
        _currentScreen = screenToSwitch;
        _currentScreen.Show();

        ChangeBackButtonText();
    }

    /// <summary>
    /// Hide current screen. Useful for popups
    /// </summary>
    public void HideCurrentScreen()
    {
        _currentScreen?.Hide();
    }
    #endregion

    #region Scene change methods
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadScene(SceneName nextSceneName)
    {
        if (nextSceneName == SceneName.MainMenuScene)
            MusicManager.Instance.PlayTitleMusic();
        SceneManager.LoadScene(nextSceneName.ToString());
    }
    #endregion

    #region Back to screen management

    public void BackToScreen()
    {
        if (!_currentScreen.HasBackButton()) return;

        HandleScreenChange(_screenStack.Pop(), true, false);
    }

    void OnEsc(InputAction.CallbackContext ctx)
    {
        BackToScreen();
    }

    void ChangeBackButtonText()
    {
        if (!_currentScreen.HasBackButton()) return;

        _currentScreen.ChangeBackButtonText(backBtnBinding[currentBinding]);
    }

    void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added || change == InputDeviceChange.Removed ||
        change == InputDeviceChange.Reconnected || change == InputDeviceChange.Disconnected)
        {
            AssignDevices();
            ChangeBackButtonText();
        }
    }

    void OnActionChange(object obj, InputActionChange change)
    {
        if (change == InputActionChange.ActionStarted)
        {
            var d = ((InputAction)obj).activeControl.device;

            if (currentBinding == 1) // else it would change onDeviceChange, only other change is if both gamepads are active but they use keyboard anyway
            {
                if (d is Keyboard)
                    currentBinding = 2;
                else
                    currentBinding = 1;
                
                ChangeBackButtonText();
            }
        }
    }

    void AssignDevices()
    {
        var pads = Gamepad.all;

        if (pads.Count == 0)
            currentBinding = 0; // keyboard
        else if (pads.Count == 1)
            currentBinding = 2; // keyboard and gamepad
        else
            currentBinding = 1; // gamepad
    }

    void InitializeDeviceBindings()
    {
        InputAction action = inputAction.actionMaps[0].FindAction("Esc");

        backBtnBinding[0] = action.GetBindingDisplayString(0); // keyboard button
        backBtnBinding[1] = action.GetBindingDisplayString(1); // gamepad button
        backBtnBinding[2] = backBtnBinding[0] + " / " + backBtnBinding[1];

        currentBinding = 0;
    }
    #endregion

    #region Handle focus
    void HandleControllerMouseNavigation()
    {
        // Monitor current selection
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            lastUsedButton = EventSystem.current.currentSelectedGameObject;
        }

        // Check for mouse input
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            if (!isUsingMouse) SwitchToMouse();
        }

        // Check for keyboard/gamepad input
        if (Input.anyKeyDown && !(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            if (isUsingMouse) SwitchToController();
        }
    }

    /// <summary>
    /// Deselects lingering button when using mouse
    /// </summary>
    private void SwitchToMouse()
    {
        isUsingMouse = true;
        Cursor.visible = true;
        EventSystem.current.SetSelectedGameObject(null);
        GetComponent<CanvasGroup>().blocksRaycasts = true; // mouse interaction
    }

    /// <summary>
    /// Selects last used button again when using gamepad/keyboard
    /// </summary>
    void SwitchToController()
    {
        isUsingMouse = false;
        Cursor.visible = false;
        EventSystem.current.SetSelectedGameObject(lastUsedButton);
        GetComponent<CanvasGroup>().blocksRaycasts = false; // deselect the mouse interaction so it doesnt hover

    }
    #endregion
}
