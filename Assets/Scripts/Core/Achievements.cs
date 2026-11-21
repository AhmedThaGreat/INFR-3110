using System.Collections.Generic;
using UnityEngine;

// Achievements / progress tracking. Another OBSERVER: it watches the game's
// events and unlocks achievements on its own. The coin, gem and GameManager
// scripts don't know achievements exist.
// When one unlocks, it announces AchievementUnlocked (so the HUD and the
// AudioManager can show it and play a sound) and saves it with PlayerPrefs,
// so unlocked achievements are remembered between play sessions.
public class Achievements : MonoBehaviour
{
    private HashSet<string> unlocked = new HashSet<string>();
    private int coinsThisRun = 0;

    void OnEnable()
    {
        GameEvents.PickupCollected += OnPickupCollected;
        GameEvents.ComboChanged += OnComboChanged;
        GameEvents.TreasureGoalReached += OnTreasureGoalReached;
        GameEvents.GameWon += OnGameWon;
    }

    void OnDisable()
    {
        GameEvents.PickupCollected -= OnPickupCollected;
        GameEvents.ComboChanged -= OnComboChanged;
        GameEvents.TreasureGoalReached -= OnTreasureGoalReached;
        GameEvents.GameWon -= OnGameWon;
    }

    void OnPickupCollected(PickupType type, Vector3 position)
    {
        if (type == PickupType.Coin)
        {
            coinsThisRun++;
            Unlock("First Doubloon", "Collect your first coin");
            if (coinsThisRun >= 25) Unlock("Deep Pockets", "Collect 25 coins in one run");
        }
        else if (type == PickupType.Gem)
        {
            Unlock("Shiny!", "Find a gem");
        }
        else if (type == PickupType.PowerUp)
        {
            Unlock("Sea Legs", "Grab a speed power-up");
        }
    }

    void OnComboChanged(int combo)
    {
        if (combo >= 5) Unlock("Combo Captain", "Reach a x5 combo");
    }

    void OnTreasureGoalReached()
    {
        Unlock("Treasure Hunter", "Unlock the treasure chest");
    }

    void OnGameWon()
    {
        Unlock("Pirate King", "Claim the treasure");

        if (GameManager.Instance != null && !GameManager.Instance.TookDamage)
        {
            Unlock("Untouchable", "Win without losing a heart");
        }
    }

    void Unlock(string title, string description)
    {
        if (unlocked.Contains(title)) return;   // already unlocked this run
        unlocked.Add(title);

        string key = "achievement_" + title;
        bool firstTimeEver = PlayerPrefs.GetInt(key, 0) == 0;
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();

        Debug.Log("Achievement: " + title + " - " + description);
        GameEvents.RaiseAchievementUnlocked(firstTimeEver ? title + " (new!)" : title);
    }
}
