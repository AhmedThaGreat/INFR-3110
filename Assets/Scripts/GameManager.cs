using UnityEngine;

// GameManager inherits from Singleton<GameManager>, so there is only one
// and any script can reach it with GameManager.Instance.
public class GameManager : Singleton<GameManager>
{
    // Tracks the player's total score. Private, so other scripts
    // have to go through AddScore().
    private int score = 0;

    // Stops the "You Win" message from firing twice for one win.
    private bool hasWon = false;

    // The native DLL keeps its combo state between Play sessions in the
    // editor, so we clear it every time the game starts.
    private void Start()
    {
        MyPlugin.ResetCombo();
    }

    // Called by Coin (and anything else that gives points).
    public void AddScore(int amount)
    {
        // The C++ DLL decides the combo multiplier (x1 to x5) based on
        // how quickly coins are being collected.
        int multiplier = MyPlugin.RegisterCoin(Time.time);

        score += amount * multiplier;
        Debug.Log("Score: " + score + "  (combo x" + multiplier + ")");
    }

    // Called when the player reaches the Goal.
    public void Win()
    {
        if (hasWon) return;   // already won, ignore the duplicate trigger
        hasWon = true;
        Debug.Log("You Win! Final Score: " + score);
    }

    // Called when the player touches a Hazard.
    public void Lose()
    {
        hasWon = false;
        MyPlugin.ResetCombo();   // a loss breaks the streak
        Debug.Log("You Lose! Try Again.");
    }
}