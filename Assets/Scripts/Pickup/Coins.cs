using UnityEngine;

// INHERITANCE: Coin gets the trigger detection and PointValue from Pickup.
public class Coin : Pickup
{
    [SerializeField] private int treasureValue = 1;   // counts toward unlocking the chest

    public override PickupType Type => PickupType.Coin;

    protected override void Collect(GameObject player)
    {
        GameManager.Instance.AddScore(PointValue);
        GameManager.Instance.AddTreasure(treasureValue);
    }
}
