using UnityEngine;
using UnityEngine.SceneManagement;

// The three situations the whole game can be in.
public enum GameState
{
    Playing,
    Won,
    Lost
}

// SINGLETON: there is only ever one GameManager, and any script can reach it
// with GameManager.Instance.
//
// What changed since the check-in:
//  - It no longer draws the HUD itself. It just keeps the numbers (score,
//    treasure, hearts, game state) and ANNOUNCES changes through GameEvents
//    (Observer). The HUD, sounds and achievements listen and react.
//  - It now tracks hearts, a treasure goal and a win/lose state.
public class GameManager : Singleton<GameManager>
{
    [Header("Rules")]
    [SerializeField] private int maxHealth = 3;        // hearts at the start
    [SerializeField] private int treasureGoal = 20;    // treasure needed to unlock the chest
    [SerializeField] private float invincibleTime = 1.5f; // seconds of safety after a hit

    public GameState State { get; private set; } = GameState.Playing;
    public int Score { get; private set; }
    public int Treasure { get; private set; }
    public int Health { get; private set; }
    public int MaxHealth => maxHealth;
    public int TreasureGoal => treasureGoal;
    public bool TookDamage { get; private set; }       // used by the "Untouchable" achievement

    private float lastHitTime = -100f;
    private bool goalAnnounced = false;

    private void Start()
    {
        // The native DLL keeps its combo between Play sessions in the editor,
        // so clear it every time the game starts.
        MyPlugin.ResetCombo();

        Health = maxHealth;

        // Tell every listener the starting values so the HUD is correct on frame 1.
        GameEvents.RaiseScoreChanged(Score);
        GameEvents.RaiseTreasureChanged(Treasure, treasureGoal);
        GameEvents.RaiseHealthChanged(Health, maxHealth);
        GameEvents.RaiseComboChanged(1);
    }

    // Called by coins and gems. The C++ plugin decides the combo multiplier.
    public void AddScore(int amount)
    {
        if (State != GameState.Playing) return;

        int multiplier = MyPlugin.RegisterCoin(Time.time);
        Score = Score + amount * multiplier;

        GameEvents.RaiseScoreChanged(Score);
        GameEvents.RaiseComboChanged(multiplier);
    }

    // Called by coins and gems. Enough treasure unlocks the chest.
    public void AddTreasure(int amount)
    {
        if (State != GameState.Playing) return;

        Treasure = Treasure + amount;
        GameEvents.RaiseTreasureChanged(Treasure, treasureGoal);

        if (Treasure >= treasureGoal && goalAnnounced == false)
        {
            goalAnnounced = true;
            GameEvents.RaiseTreasureGoalReached();
            GameEvents.RaiseMessage("The treasure chest is unlocked! Get to it!");
        }
    }

    // Called by the spikes, cannonballs and falling off the island.
    // hitFrom = where the hit came from, so the player gets knocked away from it.
    public void DamagePlayer(int amount, Vector3 hitFrom)
    {
        if (State != GameState.Playing) return;
        if (Time.time - lastHitTime < invincibleTime) return;   // still flashing from the last hit

        lastHitTime = Time.time;
        TookDamage = true;
        Health = Mathf.Max(0, Health - amount);

        // Getting hit breaks the combo streak.
        MyPlugin.ResetCombo();
        GameEvents.RaiseComboChanged(1);
        GameEvents.RaiseHealthChanged(Health, maxHealth);

        if (Health <= 0)
        {
            State = GameState.Lost;
            GameEvents.RaisePlayerDied();
        }
        else
        {
            GameEvents.RaisePlayerDamaged(hitFrom);
        }
    }

    // Called when the player touches the chest. Only wins once it is unlocked.
    public bool TryWin()
    {
        if (State != GameState.Playing) return false;

        if (Treasure < treasureGoal)
        {
            GameEvents.RaiseMessage("Locked! Collect " + (treasureGoal - Treasure) + " more treasure.");
            return false;
        }

        State = GameState.Won;
        GameEvents.RaiseGameWon();
        return true;
    }

    // Reloads the level from scratch (used by the RestartCommand).
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Kept so older scripts (PowerUp, Gem) still work. It now goes through the
    // Observer instead of drawing text itself.
    public void ShowPickupMessage(string text, float seconds = 2.5f)
    {
        GameEvents.RaiseMessage(text);
    }
}
