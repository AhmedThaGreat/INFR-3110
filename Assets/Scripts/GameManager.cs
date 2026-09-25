using UnityEngine;

// GameManager inherits from Singleton<GameManager> — that one line
// automatically gives it the "only one, reachable from anywhere"
// behavior we wrote in the base class. We didn't have to rewrite
// any of that logic here.
public class GameManager : Singleton<GameManager>
{
    // Tracks the player's total score. Private, so other scripts
    // can't change it directly — they have to go through AddScore().
    private int score = 0;

    // Any script can call this from anywhere, like:
    // GameManager.Instance.AddScore(1);
    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log("Score: " + score);
    }

    // Called when the player reaches the Goal.
    public void Win()
    {
        Debug.Log("You Win! Final Score: " + score);
    }

    // Called when the player touches a Hazard.
    public void Lose()
    {
        Debug.Log("You Lose! Try Again.");
    }
}