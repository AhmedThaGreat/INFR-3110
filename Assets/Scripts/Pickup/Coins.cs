using UnityEngine;

// INHERITANCE: Coin inherits everything from Pickup (the trigger
// detection, PointValue, etc.) instead of rewriting it from scratch.
public class Coin : Pickup
{
    // This OVERRIDES Pickup's abstract Collect() with coin-specific
    // behaviour. This is the other half of POLYMORPHISM.
    protected override void Collect(GameObject player)
    {
        Debug.Log("Collected a coin worth " + PointValue + " points!");
    }
}