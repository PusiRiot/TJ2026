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
    [SerializeField] GameObject chooseText;
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

    // text
    string[] infoBtnBinding = new string[2]; // 0 for keyboard, 1 for gamepad
    string[] readyBtnBinding = new string[2]; // 0 for keyboard, 1 for gamepad
    int currentBinding;
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
    }

    void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
        inputAction.actionMaps[_playerIndex].FindAction("Left").performed -= OnLeft;
        inputAction.actionMaps[_playerIndex].FindAction("Right").performed -= OnRight;
        inputAction.actionMaps[_playerIndex].FindAction("Info").performed -= OnInfo;
        inputAction.actionMaps[_playerIndex].FindAction("Ready").performed -= OnReady;
    }

    void Start()
    {
        screen = GetComponentInParent<UIScreenCharacterSelection>();

        inputAction.actionMaps[_playerIndex].Enable(); // Enable the first action map (you may want to specify which one if you have multiple)

        AssignDevices();

        InitializeDeviceBindings();

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
        UpdateUI(currentBinding);
    }

    #endregion

    #region Assign devices

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        AssignDevices();
        UpdateUI(currentBinding);
    }

    private void AssignDevices()
    {
        var pads = Gamepad.all;
        //Player 1
        if (_playerIndex == 0)
        {
            if (pads.Count == 0)
            {
                inputAction.actionMaps[0].devices = new InputDevice[] { Keyboard.current };
                currentBinding = 0;
            }
            else if (pads.Count == 1)
            {
                inputAction.actionMaps[0].devices = new InputDevice[] { Keyboard.current };
                currentBinding = 0;
            }
            else
            {
                inputAction.actionMaps[0].devices = new InputDevice[] { Keyboard.current, pads[1] };
                currentBinding = 1;
            }
        }
        //Player 2
        if (_playerIndex == 1)
        {
            if (pads.Count == 0)
            {
                inputAction.actionMaps[1].devices = new InputDevice[] { Keyboard.current };
                currentBinding = 0;
            }
            else
            {
                inputAction.actionMaps[1].devices = new InputDevice[] { Keyboard.current, pads[0] };
                currentBinding = 1;
            }
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
        if (!ctx.performed || (!infoShown && ready)) return;

        UpdateUI(ctx.action.activeControl.device.name == "Keyboard" ? 0 : 1);

        if (!infoShown)
        {
            characterButtons[currentButtonIndex].Deselect(_playerIndex);

            currentButtonIndex = (currentButtonIndex - 1 + characterButtons.Length) % characterButtons.Length;
            var selectedCharacter = characterButtons[currentButtonIndex].Select(_playerIndex);
            currentCharacter = selectedCharacter;
            AssignCharacterScreen(selectedCharacter);

            //Audio
            AkUnitySoundEngine.PostEvent(GetHoverEvent(selectedCharacter), gameObject);
        }
        else
        {
            var selectedCharacter = characterButtons[currentButtonIndex].Select(_playerIndex);
            characterScreens[selectedCharacter == PlayerCharacter.Peggy ? 0 : 1].ChangeInfoButton(true);
        }
        
    }

    public void OnRight(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || (!infoShown && ready)) return;

        UpdateUI(ctx.action.activeControl.device.name == "Keyboard" ? 0 : 1);

        if (!infoShown)
        {        
            characterButtons[currentButtonIndex].Deselect(_playerIndex);

            currentButtonIndex = (currentButtonIndex + 1) % characterButtons.Length;
            var selectedCharacter = characterButtons[currentButtonIndex].Select(_playerIndex);
            currentCharacter = selectedCharacter;
            AssignCharacterScreen(selectedCharacter);
        
            //Audio
            AkUnitySoundEngine.PostEvent(GetHoverEvent(selectedCharacter), gameObject);
        }
        else
        {
            var selectedCharacter = characterButtons[currentButtonIndex].Select(_playerIndex);
            characterScreens[selectedCharacter == PlayerCharacter.Peggy ? 0 : 1].ChangeInfoButton(false);
            
        }
    }

    public void OnInfo(InputAction.CallbackContext ctx)
    {
        infoShown = !infoShown;
        foreach(var characterScreen in characterScreens)
        {
            characterScreen.ShowInfo(infoShown);
        }
        UpdateUI(ctx.action.activeControl.device.name == "Keyboard" ? 0 : 1);
    }

    public void OnReady(InputAction.CallbackContext ctx)
    {
        ready = !ready;

        //Audio
        if (ready)
            AkUnitySoundEngine.PostEvent(GetSelectEvent(currentCharacter), gameObject);

        readyImage.SetActive(ready);
        UpdateUI(ctx.action.activeControl.device.name == "Keyboard" ? 0 : 1);
        PlayerReadyChanged?.Invoke(ready);
    }
    #endregion

    #region Player input text update
    void InitializeDeviceBindings()
    {
        InputAction action = inputAction.actionMaps[_playerIndex].FindAction("Info");
        infoBtnBinding[0] = action.GetBindingDisplayString(0); // keyboard button
        infoBtnBinding[1] = action.GetBindingDisplayString(1); // gamepad button

        action = inputAction.actionMaps[_playerIndex].FindAction("Ready");
        readyBtnBinding[0] = action.GetBindingDisplayString(0); // keyboard button
        readyBtnBinding[1] = action.GetBindingDisplayString(1); // gamepad button

        currentBinding = 0;
    }

    private void UpdateUI(int binding)
    {
        string infoKey = infoBtnBinding[binding];
        string readyKey = readyBtnBinding[binding];

        if (binding == 0)
            chooseText.SetActive(true);
        else 
            chooseText.SetActive(false);

        if (infoShown)
            infoText.text = $"{infoKey} - Info";
        else
            infoText.text = $"{infoKey} - Info";

        if (ready)
            readyText.text = $"{readyKey} - Unready";
        else
            readyText.text = $"{readyKey} - Ready";
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