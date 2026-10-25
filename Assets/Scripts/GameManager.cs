using UnityEngine;

// GameManager inherits from Singleton<GameManager>, so there is only one
// and any script can reach it with GameManager.Instance.
public class GameManager : Singleton<GameManager>
{
    private int score = 0;

    // Stops the "You Win" message from firing twice for one win.
    private bool hasWon = false;

    // Used only to DISPLAY the combo on screen (the C++ plugin decides it).
    private int lastMultiplier = 1;
    private float lastCoinTime = -100f;

    // The message shown in the middle of the screen (win / lose).
    private string message = "";
    private float messageUntil = 0f;

    // The native DLL keeps its combo state between Play sessions in the
    // editor, so we clear it every time the game starts.
    private void Start()
    {
        MyPlugin.ResetCombo();
    }

    // Called by Coin and Gem (anything that gives points).
    public void AddScore(int amount)
    {
        // The C++ DLL decides the combo multiplier (x1 to x5).
        int multiplier = MyPlugin.RegisterCoin(Time.time);

        lastMultiplier = multiplier;
        lastCoinTime = Time.time;
        score += amount * multiplier;
        Debug.Log("Score: " + score + "  (combo x" + multiplier + ")");
    }

    // Called when the player reaches the Goal.
    public void Win()
    {
        if (hasWon) return;   // already won, ignore the duplicate trigger
        hasWon = true;
        message = "You Win!  Final Score: " + score;
        messageUntil = float.MaxValue;   // stays on screen
        Debug.Log("You Win! Final Score: " + score);
    }

    // Called when the player touches a Hazard.
    public void Lose()
    {
        hasWon = false;
        MyPlugin.ResetCombo();   // a loss breaks the streak
        lastMultiplier = 1;
        message = "You Lose!  Try Again.";
        messageUntil = Time.time + 3f;   // shown for 3 seconds
        Debug.Log("You Lose! Try Again.");
    }

    // Draws simple text on screen. Unity calls this automatically.
    private void OnGUI()
    {
        int size = Mathf.Max(18, Screen.height / 28);

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = size;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;

        // Show the combo only while it is still "alive" (3 seconds).
        int shownCombo = (Time.time - lastCoinTime <= 3f) ? lastMultiplier : 1;
        GUI.Label(new Rect(20, 20, 600, size * 2),
                  "Score: " + score + "     Combo x" + shownCombo, style);

        // Win / lose message in the middle of the screen.
        if (message != "" && Time.time < messageUntil)
        {
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = size * 2;
            GUI.Label(new Rect(0, Screen.height * 0.35f, Screen.width, size * 3),
                      message, style);
        }
    }
}