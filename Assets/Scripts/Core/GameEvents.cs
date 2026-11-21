using System;
using UnityEngine;

// OBSERVER PATTERN - the "subject" (the thing that announces news).
//
// Scripts that make things happen (GameManager, pickups, the cannon) only
// ANNOUNCE what happened by calling a Raise...() method.
// Scripts that react (HUD, AudioManager, Achievements, the treasure chest,
// the player's state machine) SUBSCRIBE to the events they care about.
//
// Nobody needs a reference to anybody else, so I can add a new listener
// (like a sound or an achievement) without touching the code that raises it.
//
// Rule: every listener subscribes in OnEnable and unsubscribes in OnDisable,
// so nothing is left listening after the scene reloads.
public static class GameEvents
{
    // ---- Score and treasure ----
    public static event Action<int> ScoreChanged;              // new total score
    public static event Action<int, int> TreasureChanged;      // treasure collected, treasure goal
    public static event Action<int> ComboChanged;              // current combo multiplier (1 to 5)
    public static event Action<PickupType, Vector3> PickupCollected; // what was picked up, and where
    public static event Action TreasureGoalReached;            // enough treasure: the chest unlocks

    // ---- Player health ----
    public static event Action<int, int> HealthChanged;        // current hearts, max hearts
    public static event Action<Vector3> PlayerDamaged;         // where the hit came from
    public static event Action PlayerDied;
    public static event Action<string> PlayerStateChanged;     // name of the player's new state

    // ---- Game flow ----
    public static event Action GameWon;
    public static event Action<string> Message;                // short message for the HUD
    public static event Action<string> AchievementUnlocked;    // achievement title
    public static event Action<Vector3> CannonFired;           // where the cannon fired from

    // ---- Raise methods (the only way to announce something) ----
    public static void RaiseScoreChanged(int score) => ScoreChanged?.Invoke(score);
    public static void RaiseTreasureChanged(int collected, int goal) => TreasureChanged?.Invoke(collected, goal);
    public static void RaiseComboChanged(int combo) => ComboChanged?.Invoke(combo);
    public static void RaisePickupCollected(PickupType type, Vector3 position) => PickupCollected?.Invoke(type, position);
    public static void RaiseTreasureGoalReached() => TreasureGoalReached?.Invoke();

    public static void RaiseHealthChanged(int current, int max) => HealthChanged?.Invoke(current, max);
    public static void RaisePlayerDamaged(Vector3 hitFrom) => PlayerDamaged?.Invoke(hitFrom);
    public static void RaisePlayerDied() => PlayerDied?.Invoke();
    public static void RaisePlayerStateChanged(string stateName) => PlayerStateChanged?.Invoke(stateName);

    public static void RaiseGameWon() => GameWon?.Invoke();
    public static void RaiseMessage(string text) => Message?.Invoke(text);
    public static void RaiseAchievementUnlocked(string title) => AchievementUnlocked?.Invoke(title);
    public static void RaiseCannonFired(Vector3 position) => CannonFired?.Invoke(position);
}
