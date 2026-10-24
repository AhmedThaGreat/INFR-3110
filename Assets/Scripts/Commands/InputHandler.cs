using System;
using System.Collections.Generic;
using UnityEngine;

// The "Invoker" in the Command pattern.
// It reads the keyboard and runs the command linked to each key.
// It never moves the player itself, it only calls Execute() on a command.
public class InputHandler : MonoBehaviour
{
    public PlayerMovement player; // drag the Player object here in the Inspector

    // Keys that repeat every frame while held (walking).
    private Dictionary<KeyCode, ICommand> holdBindings = new Dictionary<KeyCode, ICommand>();

    // Keys that run once when pressed (jumping).
    private Dictionary<KeyCode, ICommand> pressBindings = new Dictionary<KeyCode, ICommand>();

    private KeyCode jumpKey = KeyCode.Space; // the key currently linked to Jump
    private ICommand jumpCommand;            // the Jump command object
    private bool waitingForNewKey = false;   // true after F2 until a new key is pressed

    void Start()
    {
        // Make the four walking commands once, then link keys to them.
        ICommand forward = new MoveCommand(player, 0f, 1f);
        ICommand back = new MoveCommand(player, 0f, -1f);
        ICommand left = new MoveCommand(player, -1f, 0f);
        ICommand right = new MoveCommand(player, 1f, 0f);

        holdBindings[KeyCode.W] = forward;
        holdBindings[KeyCode.UpArrow] = forward;
        holdBindings[KeyCode.S] = back;
        holdBindings[KeyCode.DownArrow] = back;
        holdBindings[KeyCode.A] = left;
        holdBindings[KeyCode.LeftArrow] = left;
        holdBindings[KeyCode.D] = right;
        holdBindings[KeyCode.RightArrow] = right;

        // Link the Jump key to the Jump command.
        jumpCommand = new JumpCommand(player);
        pressBindings[jumpKey] = jumpCommand;
    }

    void Update()
    {
        // While rebinding, only listen for the new key.
        if (waitingForNewKey)
        {
            ListenForNewKey();
            return;
        }

        // F2 starts rebinding the Jump key.
        if (Input.GetKeyDown(KeyCode.F2))
        {
            waitingForNewKey = true;
            Debug.Log("Press the new key for Jump (Esc to cancel)...");
            return;
        }

        // Run every walking command whose key is being held.
        foreach (KeyValuePair<KeyCode, ICommand> binding in holdBindings)
        {
            if (Input.GetKey(binding.Key)) binding.Value.Execute();
        }

        // Run every command whose key was just pressed.
        foreach (KeyValuePair<KeyCode, ICommand> binding in pressBindings)
        {
            if (Input.GetKeyDown(binding.Key)) binding.Value.Execute();
        }
    }

    // Waits for the next key press and links it to the Jump command.
    void ListenForNewKey()
    {
        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if (!Input.GetKeyDown(key)) continue;

            if (key == KeyCode.Escape)
            {
                waitingForNewKey = false;
                Debug.Log("Rebind cancelled. Jump is still " + jumpKey);
            }
            else if (holdBindings.ContainsKey(key))
            {
                Debug.Log(key + " is used for walking. Press a different key.");
            }
            else
            {
                pressBindings.Remove(jumpKey);       // unlink the old key
                jumpKey = key;
                pressBindings[jumpKey] = jumpCommand; // link the new key
                waitingForNewKey = false;
                Debug.Log("Jump is now " + jumpKey);
            }
            return;
        }
    }
}