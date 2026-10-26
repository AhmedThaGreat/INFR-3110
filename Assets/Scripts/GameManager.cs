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

    // A small message under the score (power-up / gem collected).
    private string pickupMessage = "";
    private float pickupMessageUntil = 0f;

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

    // Called by pickups (Gem, PowerUp) to show a short message on screen.
    public void ShowPickupMessage(string text, float seconds = 2.5f)
    {
        pickupMessage = text;
        pickupMessageUntil = Time.time + seconds;
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
        style.wordWrap = false;   // never wrap the score onto a second line

        // Show the combo only while it is still "alive" (3 seconds).
        int shownCombo = (Time.time - lastCoinTime <= 3f) ? lastMultiplier : 1;
        GUI.Label(new Rect(20, 20, Screen.width - 40, size * 2),
                  "Score: " + score + "     Combo x" + shownCombo, style);

        // Small pickup message under the score (power-up / gem).
        if (pickupMessage != "" && Time.time < pickupMessageUntil)
        {
            GUI.Label(new Rect(20, 20 + size * 1.6f, Screen.width - 40, size * 2),
                      pickupMessage, style);
        }

        // Win / lose message in the middle of the screen.
        if (message != "" && Time.time < messageUntil)
        {
            style.alignment = TextAnchor.MiddleCenter;
            style.wordWrap = true;
            style.fontSize = size * 2;
            GUI.Label(new Rect(0, Screen.height * 0.35f, Screen.width, size * 3),
                      message, style);
        }
    }
}
