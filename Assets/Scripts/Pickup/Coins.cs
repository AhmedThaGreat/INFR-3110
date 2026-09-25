using UnityEngine;

// INHERITANCE: Coin inherits everything from Pickup (the trigger
// detection, PointValue, etc.) instead of rewriting it from scratch.
public class Coin : Pickup
{
    // This OVERRIDES Pickup's abstract Collect() with coin-specific
    // behaviour. This is the other half of POLYMORPHISM.
    protected override void Collect(GameObject player)
    {
        // Instead of just logging, we now tell GameManager to actually
        // track the score. Since GameManager is a Singleton, we can
        // reach it from anywhere with GameManager.Instance — no need
        // to manually drag-and-drop a reference in the Inspector.
        GameManager.Instance.AddScore(PointValue);
    }
}