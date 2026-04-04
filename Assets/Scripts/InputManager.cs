using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    PlayerInput p1Input;
    PlayerInput p2Input;

    void Awake()
    {
        // TODO: assign tags before this
        p1Input = GameObject.FindGameObjectWithTag("Player1").GetComponentInChildren<PlayerInput>();
        p2Input = GameObject.FindGameObjectWithTag("Player2").GetComponentInChildren<PlayerInput>();

        // Run the setup once when the game starts
        AssignDevices();
    }

    void OnEnable()
    {
        // Subscribe to device changes when this script is active
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        // We only want to rearrange things if a Gamepad was added or removed
        if (device is Gamepad)
        {
            if (change == InputDeviceChange.Added ||
                change == InputDeviceChange.Disconnected ||
                change == InputDeviceChange.Removed)
            {
                Debug.Log($"Gamepad {change}! Rearranging inputs...");
                AssignDevices();
            }
        }
    }


    private void AssignDevices()
    {
        var pads = Gamepad.all;

        // SCENARIO 1: Two Controllers
        if (pads.Count >= 2)
        {
            p1Input.SwitchCurrentControlScheme("Controller", pads[0]);
            p2Input.SwitchCurrentControlScheme("Controller", pads[1]);
            Debug.Log("1");
        }
        // SCENARIO 2: One Controller, One Keyboard
        else if (pads.Count == 1)
        {
            p1Input.SwitchCurrentControlScheme("Keyboard1", Keyboard.current);
            p2Input.SwitchCurrentControlScheme("Controller", pads[0]);
            Debug.Log("2");
        }
        // SCENARIO 3: Two Players on one Keyboard
        else
        {
            p1Input.SwitchCurrentControlScheme("Keyboard1", Keyboard.current);
            p2Input.SwitchCurrentControlScheme("Keyboard2", Keyboard.current);
            Debug.Log("3");
        }
    }
}