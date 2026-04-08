using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    PlayerInput p1Input;
    PlayerInput p2Input;

    void Awake()
    {
        p1Input = GameObject.FindGameObjectWithTag("Player1").GetComponentInChildren<PlayerInput>();
        p2Input = GameObject.FindGameObjectWithTag("Player2").GetComponentInChildren<PlayerInput>();

        AssignDevices();
    }

    void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Gamepad)
        {
            AssignDevices();
            Debug.Log("Gamepad Change Detected: " + change.ToString() + ". Reassinging control schemes! :)");
        }
    }

    private void AssignDevices()
    {
        var pads = Gamepad.all;

        // SCENARIO 1: Two Controllers
        if (pads.Count >= 2)
        {
            // Give P1 the keyboard AND Pad 0
            p1Input.SwitchCurrentControlScheme("P1Scheme", Keyboard.current, pads[1]);

            // Give P2 the keyboard AND Pad 1
            p2Input.SwitchCurrentControlScheme("P2Scheme", Keyboard.current, pads[0]);
        }
        // SCENARIO 2: One Controller, One Keyboard
        else if (pads.Count == 1)
        {
            // P1 gets JUST the keyboard
            p1Input.SwitchCurrentControlScheme("P1Scheme", Keyboard.current);

            // P2 gets the keyboard AND the single controller
            p2Input.SwitchCurrentControlScheme("P2Scheme", Keyboard.current, pads[0]);
        }
        // SCENARIO 3: Two Players on one Keyboard
        else
        {
            // Both get the keyboard, no controllers assigned
            p1Input.SwitchCurrentControlScheme("P1Scheme", Keyboard.current);
            p2Input.SwitchCurrentControlScheme("P2Scheme", Keyboard.current);
        }
    }
}