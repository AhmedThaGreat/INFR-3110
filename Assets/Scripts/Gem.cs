using UnityEngine;

// Gem inherits from Pickup, just like Coin and PowerUp.
// It gets the trigger detection and PointValue for free.
public class Gem : Pickup
{
    // Same idea as Coin: give points to the GameManager.
    // The Gem's higher value (5) is set on its prefab in the Inspector.
    protected override void Collect(GameObject player)
    {
        GameManager.Instance.AddScore(PointValue);
    }
}
