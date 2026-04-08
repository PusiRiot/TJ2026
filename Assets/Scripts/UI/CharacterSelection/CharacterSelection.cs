using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// This is the character selection screen for each player, where they can see their current selected character, show/hide character info and ready/unready for the game start.
/// </summary>
public class CharacterSelection : MonoBehaviour
{
    int _playerIndex; // 0 for P1, 1 for P2
    [SerializeField] InputActionAsset playerInput; // Assign P1 or P2 PlayerInput
    [SerializeField] TextMeshProUGUI infoText;
    [SerializeField] TextMeshProUGUI readyText;
    CharacterScreen[] characterScreens; // Reference to the character screen to update character info
    CharacterButton[] characterButtons;
    int currentIndex;
    [SerializeField] GameObject readyImage; // Reference to the character screen to update character info
    PlayerCharacter currentCharacter; // Reference to the character screen to update character info
    bool infoShown = false;
    bool ready = false;
    public bool PlayerReady => ready;
    public UnityEvent<bool> PlayerReadyChanged;
    InputDevice lastDevice;

    void OnEnable()
    {
        _playerIndex = gameObject.CompareTag("Player1") ? 0 : 1; // it is important that this goes onEnable because it must be initialized, and OnEnable is before Start
        InputSystem.onDeviceChange += OnDeviceChange; 
        playerInput.actionMaps[_playerIndex].FindAction("Left").performed += OnLeft;
        playerInput.actionMaps[_playerIndex].FindAction("Right").performed += OnRight;
        playerInput.actionMaps[_playerIndex].FindAction("Info").performed += OnInfo;
        playerInput.actionMaps[_playerIndex].FindAction("Ready").performed += OnReady;
    }

    void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
        playerInput.actionMaps[_playerIndex].FindAction("Left").performed -= OnLeft;
        playerInput.actionMaps[_playerIndex].FindAction("Right").performed -= OnRight;
        playerInput.actionMaps[_playerIndex].FindAction("Info").performed -= OnInfo;
        playerInput.actionMaps[_playerIndex].FindAction("Ready").performed -= OnReady;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        UpdateUI(device.name);
        AssignDevices();
        
    }
    private void AssignDevices()
    {
        var pads = Gamepad.all;
        // Player 2
        if (_playerIndex == 1)
        {
            if (pads.Count == 0)
                playerInput.actionMaps[1].devices = new InputDevice[] { Keyboard.current };
            else if (pads.Count >= 1)
                playerInput.actionMaps[1].devices = new InputDevice[] { Keyboard.current, pads[0] };

        }

        // Player 1
        if (_playerIndex == 0)
        {
            if (pads.Count <= 1)
                playerInput.actionMaps[0].devices = new InputDevice[] { Keyboard.current };
            else
                playerInput.actionMaps[0].devices = new InputDevice[] { Keyboard.current, pads[1] };
        }
    }


    // this cannot go on Awake because other scripts must initialize before
    void Start()
    {
        playerInput.actionMaps[_playerIndex].Enable(); // Enable the first action map (you may want to specify which one if you have multiple)

        AssignDevices();

        // Get references to character screens and buttons
        characterScreens = GetComponentsInChildren<CharacterScreen>(true);
        characterButtons = FindObjectsByType<CharacterButton>(FindObjectsSortMode.None);

        // Assign the selected character to the character screen
        currentCharacter = GameGlobalSettings.Instance.GetPlayerCharacter(_playerIndex);
        AssignCharacterScreen(currentCharacter);

        // Assign the first character button as selected for the player
        for (int i = 0; i < characterButtons.Length; i++)
        {
            if (characterButtons[i].CharacterReference == currentCharacter)
            {
                currentIndex = i;
                characterButtons[i].Select(_playerIndex);
                break;
            }
        }

        // not ready at start
        readyImage.SetActive(false);

        // Update UI text based on the current control scheme
        UpdateUI("keyboard");
    }

    public void AssignCharacterScreen(PlayerCharacter character)
    {
        GameGlobalSettings.Instance.SetPlayerCharacter(_playerIndex, character);

        foreach (var characterScreen in characterScreens)
        {
            if (characterScreen.CharacterReference == character)
                characterScreen.gameObject.SetActive(true);
            else
                characterScreen.gameObject.SetActive(false);
        }
    }

    #region Buttons

    public void OnLeft(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        UpdateUI(ctx.action.activeControl.device.name);
        characterButtons[currentIndex].Deselect(_playerIndex);

        currentIndex = (currentIndex - 1 + characterButtons.Length) % characterButtons.Length;
        var selectedCharacter = characterButtons[currentIndex].Select(_playerIndex);
        AssignCharacterScreen(selectedCharacter);
    }

    public void OnRight(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        UpdateUI(ctx.action.activeControl.device.name);
        characterButtons[currentIndex].Deselect(_playerIndex);

        currentIndex = (currentIndex + 1) % characterButtons.Length;
        var selectedCharacter = characterButtons[currentIndex].Select(_playerIndex);
        AssignCharacterScreen(selectedCharacter);
    }

    public void OnInfo(InputAction.CallbackContext ctx)
    {
        infoShown = !infoShown;
        foreach(var characterScreen in characterScreens)
        {
            characterScreen.ShowInfo(infoShown);
        }
        UpdateUI(ctx.action.activeControl.device.name);
    }


    public void OnReady(InputAction.CallbackContext ctx)
    {
        ready = !ready;
        readyImage.SetActive(ready);
        UpdateUI(ctx.action.activeControl.device.name);
        PlayerReadyChanged?.Invoke(ready);
    }
    #endregion

    #region Player input text update

    private string GetBindingForCurrentDevice(string actionName, string layout)
    {
        InputAction action = playerInput.actionMaps[_playerIndex].FindAction(actionName);

        // binding index always has to be set up so 0 is for keyboard and 1 for gamepad
        int bindingIndex = 0;
        if (!layout.Contains("keyboard", System.StringComparison.OrdinalIgnoreCase))
            bindingIndex = 1;

        return action.GetBindingDisplayString(bindingIndex);
    }

    private void UpdateUI(string layout)
    {
        if (layout == null) return; 
        string infoKey = GetBindingForCurrentDevice("Info", layout);   // Your action name
        string readyKey = GetBindingForCurrentDevice("Ready", layout); // Your action name

        if (infoShown)
            infoText.text = $"{infoKey} - Info";
        else 
            infoText.text = $"{infoKey} - Hide Info";

        if (ready)
            readyText.text = $"{readyKey} - Ready";
        else
            readyText.text = $"{readyKey} - Unready";
    }
    #endregion
}