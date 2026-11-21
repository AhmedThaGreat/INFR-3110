using System;
using System.Collections.Generic;
using UnityEngine;

// The "Invoker" in the Command pattern.
// It watches the keyboard and runs the command linked to each key.
// It never moves the player itself, it only calls Execute() / Undo().
//
// What changed since the check-in (feedback: "doing it every frame is a bit silly"):
//  - Before: every walking command ran Execute() every single frame a key was held.
//  - Now:    a command runs Execute() ONCE when its key goes down and Undo()
//            ONCE when it comes up. Nothing runs while you just hold a key.
//  - New RestartCommand on R (the pattern reused for a game action).
//  - Rebind messages show in the HUD (through the Observer), not only the Console.
public class InputHandler : MonoBehaviour
{
    public PlayerMovement player; // drag the Player object here in the Inspector

    // Every key and the command it is linked to.
    private Dictionary<KeyCode, ICommand> bindings = new Dictionary<KeyCode, ICommand>();

    // Keys whose command is "on" right now, so Undo() only ever runs for a
    // key we actually pressed.
    private HashSet<KeyCode> keysDown = new HashSet<KeyCode>();

    private KeyCode jumpKey = KeyCode.Space; // the key currently linked to Jump
    private ICommand jumpCommand;
    private bool waitingForNewKey = false;   // true after F2 until a new key is pressed

    void Start()
    {
        ICommand forward = new MoveCommand(player, 0f, 1f);
        ICommand back = new MoveCommand(player, 0f, -1f);
        ICommand left = new MoveCommand(player, -1f, 0f);
        ICommand right = new MoveCommand(player, 1f, 0f);

        bindings[KeyCode.W] = forward;
        bindings[KeyCode.UpArrow] = forward;
        bindings[KeyCode.S] = back;
        bindings[KeyCode.DownArrow] = back;
        bindings[KeyCode.A] = left;
        bindings[KeyCode.LeftArrow] = left;
        bindings[KeyCode.D] = right;
        bindings[KeyCode.RightArrow] = right;

        jumpCommand = new JumpCommand(player);
        bindings[jumpKey] = jumpCommand;

        bindings[KeyCode.R] = new RestartCommand();
    }

    void Update()
    {
        if (waitingForNewKey)
        {
            ListenForNewKey();
            return;
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            ReleaseAllKeys();   // stop walking while choosing a key
            waitingForNewKey = true;
            Say("Press the new key for Jump (Esc to cancel)...");
            return;
        }

        foreach (KeyValuePair<KeyCode, ICommand> binding in bindings)
        {
            // Key just went down: run the command once.
            if (Input.GetKeyDown(binding.Key) && !keysDown.Contains(binding.Key))
            {
                keysDown.Add(binding.Key);
                binding.Value.Execute();
            }
            // Key just came up: undo it once.
            else if (Input.GetKeyUp(binding.Key) && keysDown.Contains(binding.Key))
            {
                keysDown.Remove(binding.Key);
                binding.Value.Undo();
            }
        }
    }

    // If the game window loses focus, Unity never sees the keys come up,
    // so undo everything to stop the player walking on its own.
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) ReleaseAllKeys();
    }

    void ReleaseAllKeys()
    {
        foreach (KeyCode key in keysDown)
        {
            if (bindings.ContainsKey(key)) bindings[key].Undo();
        }
        keysDown.Clear();
    }

    // Waits for the next key press and links it to the Jump command.
    void ListenForNewKey()
    {
        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if (!Input.GetKeyDown(key)) continue;
            if (key >= KeyCode.Mouse0 && key <= KeyCode.Mouse6) continue;   // right mouse is for looking

            if (key == KeyCode.Escape)
            {
                waitingForNewKey = false;
                Say("Rebind cancelled. Jump is still " + jumpKey);
            }
            else if (bindings.ContainsKey(key) && bindings[key] != jumpCommand)
            {
                Say(key + " is already used. Press a different key.");
            }
            else
            {
                bindings.Remove(jumpKey);        // unlink the old key
                jumpKey = key;
                bindings[jumpKey] = jumpCommand; // link the new key
                waitingForNewKey = false;
                Say("Jump is now " + jumpKey);
            }
            return;
        }
    }

    void Say(string text)
    {
        Debug.Log(text);
        GameEvents.RaiseMessage(text);
    }
}
