using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// This is the character selection screen for each player, where they can see their current selected character, show/hide character info and ready/unready for the game start.
/// </summary>
public class CharacterSelection : MonoBehaviour
{
    #region Variables
    int _playerIndex; // 0 for P1, 1 for P2

    [SerializeField] InputActionAsset inputAction;
    [SerializeField] TextMeshProUGUI infoText;
    [SerializeField] TextMeshProUGUI readyText;
    [SerializeField] GameObject readyImage; // ready image
    UIScreenCharacterSelection screen; // Reference to the character screen to update character info

    CharacterScreen[] characterScreens; // Reference to the character screen to update character info
    CharacterButton[] characterButtons;
    int currentButtonIndex;
    PlayerCharacter currentCharacter; // Reference to the character screen to update character info

    bool infoShown = false;
    bool ready = false;
    public bool PlayerReady => ready;
    public UnityEvent<bool> PlayerReadyChanged; // screen subscribes to listen when player ready state changes to start game scene
    #endregion

    #region MonoBehaviour

    void OnEnable()
    {
        _playerIndex = gameObject.CompareTag("Player1") ? 0 : 1; // it is important that this goes onEnable because it must be initialized, and OnEnable is before Start
        InputSystem.onDeviceChange += OnDeviceChange; 
        inputAction.actionMaps[_playerIndex].FindAction("Left").performed += OnLeft;
        inputAction.actionMaps[_playerIndex].FindAction("Right").performed += OnRight;
        inputAction.actionMaps[_playerIndex].FindAction("Info").performed += OnInfo;
        inputAction.actionMaps[_playerIndex].FindAction("Ready").performed += OnReady;
        inputAction.actionMaps[_playerIndex].FindAction("Esc").performed += OnEsc;
    }

    void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
        inputAction.actionMaps[_playerIndex].FindAction("Left").performed -= OnLeft;
        inputAction.actionMaps[_playerIndex].FindAction("Right").performed -= OnRight;
        inputAction.actionMaps[_playerIndex].FindAction("Info").performed -= OnInfo;
        inputAction.actionMaps[_playerIndex].FindAction("Ready").performed -= OnReady;
        inputAction.actionMaps[_playerIndex].FindAction("Esc").performed -= OnEsc;
    }

    void Start()
    {
        screen = GetComponentInParent<UIScreenCharacterSelection>();

        inputAction.actionMaps[_playerIndex].Enable(); // Enable the first action map (you may want to specify which one if you have multiple)

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
                currentButtonIndex = i;
                characterButtons[i].Select(_playerIndex);
                break;
            }
        }

        // not ready at start
        readyImage.SetActive(false);

        // Update UI text based on the current control scheme
        UpdateUI("keyboard");
    }

    #endregion

    #region Assign devices

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
                inputAction.actionMaps[1].devices = new InputDevice[] { Keyboard.current };
            else if (pads.Count >= 1)
                inputAction.actionMaps[1].devices = new InputDevice[] { Keyboard.current, pads[0] };

        }

        // Player 1
        if (_playerIndex == 0)
        {
            if (pads.Count <= 1)
                inputAction.actionMaps[0].devices = new InputDevice[] { Keyboard.current };
            else
                inputAction.actionMaps[0].devices = new InputDevice[] { Keyboard.current, pads[1] };
        }
    }
    #endregion

    #region Navigation and character screen assignment
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

    public void OnLeft(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        UpdateUI(ctx.action.activeControl.device.name);
        characterButtons[currentButtonIndex].Deselect(_playerIndex);

        currentButtonIndex = (currentButtonIndex - 1 + characterButtons.Length) % characterButtons.Length;
        var selectedCharacter = characterButtons[currentButtonIndex].Select(_playerIndex);
        currentCharacter = selectedCharacter;
        AssignCharacterScreen(selectedCharacter);

        //Audio
        AkUnitySoundEngine.PostEvent(GetHoverEvent(selectedCharacter), gameObject);
    }

    public void OnRight(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        UpdateUI(ctx.action.activeControl.device.name);
        characterButtons[currentButtonIndex].Deselect(_playerIndex);

        currentButtonIndex = (currentButtonIndex + 1) % characterButtons.Length;
        var selectedCharacter = characterButtons[currentButtonIndex].Select(_playerIndex);
        currentCharacter = selectedCharacter;
        AssignCharacterScreen(selectedCharacter);
        
        //Audio
        AkUnitySoundEngine.PostEvent(GetHoverEvent(selectedCharacter), gameObject);
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

        //Audio
        if (ready)
            AkUnitySoundEngine.PostEvent(GetSelectEvent(currentCharacter), gameObject);

        readyImage.SetActive(ready);
        UpdateUI(ctx.action.activeControl.device.name);
        PlayerReadyChanged?.Invoke(ready);
    }

    public void OnEsc(InputAction.CallbackContext ctx)
    {
        UINavigationManager.Instance.ShowScreen(ScreenName.MainMenu, true);
    }
    #endregion

    #region Player input text update

    private void UpdateUI(string layout)
    {
        if (layout == null) return; 
        string infoKey = GetBindingForCurrentDevice("Info", layout);   // Your action name
        string readyKey = GetBindingForCurrentDevice("Ready", layout); // Your action name
        string escKey = GetBindingForCurrentDevice("Esc", layout); // Your action name

        if(infoText == null) {
            Debug.Log("?");
        }

        if (infoShown)
            infoText.text = $"{infoKey} - Skill Info";
        else 
            infoText.text = $"{infoKey} - Flashlight Info";

        if (ready)
            readyText.text = $"{readyKey} - Unready";
        else
            readyText.text = $"{readyKey} - Ready";

        screen.ChangeEscText(_playerIndex, $"{escKey}");
    }

    private string GetBindingForCurrentDevice(string actionName, string layout)
    {
        InputAction action = inputAction.actionMaps[_playerIndex].FindAction(actionName);

        // binding index always has to be set up so 0 is for keyboard and 1 for gamepad
        int bindingIndex = 0;
        if (!layout.Contains("keyboard", System.StringComparison.OrdinalIgnoreCase))
            bindingIndex = 1;

        return action.GetBindingDisplayString(bindingIndex);
    }
    #endregion

    #region Audio

    private string GetHoverEvent(PlayerCharacter character)
    {
        return character switch
        {
            PlayerCharacter.Peggy => "Peggy_Hover",
            PlayerCharacter.DrHives => "Dr_Hives_Hover",
            _ => ""
        };
    }

    private string GetSelectEvent(PlayerCharacter character)
    {
        return character switch
        {
            PlayerCharacter.Peggy => "Peggy_Select",
            PlayerCharacter.DrHives => "Dr_Hives_Select",
            _ => ""
        };
    }


    #endregion
}