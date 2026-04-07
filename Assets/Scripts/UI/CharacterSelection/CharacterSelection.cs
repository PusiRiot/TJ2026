using System;
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
    }

    // this cannot go on Awake because other scripts must initialize before
    void Start()
    {
        playerInput.actionMaps[_playerIndex].Enable(); // Enable the first action map (you may want to specify which one if you have multiple)

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

        characterButtons[currentIndex].Deselect(_playerIndex);

        currentIndex = (currentIndex - 1 + characterButtons.Length) % characterButtons.Length;
        var selectedCharacter = characterButtons[currentIndex].Select(_playerIndex);
        AssignCharacterScreen(selectedCharacter);
    }

    public void OnRight(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

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

        int bindingIndex = GetBindingIndexForDevice(action, layout);

        if (bindingIndex < 0)
            return "N/A";

        return action.GetBindingDisplayString(bindingIndex);
    }

    private int GetBindingIndexForDevice(InputAction action, string layout)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];

            // Skip composites like "2DVector"
            if (binding.isComposite || binding.isPartOfComposite)
                continue;

            // Match device layout
            if (binding.path.Contains(layout, System.StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    private void UpdateUI(string layout)
    {
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