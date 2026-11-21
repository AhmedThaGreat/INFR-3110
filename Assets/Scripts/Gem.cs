using UnityEngine;

// Gem inherits from Pickup, just like Coin and PowerUp.
// It is rarer (the factory's weights decide that), worth more points
// (set on the prefab, 5) and counts as more treasure.
public class Gem : Pickup
{
    [SerializeField] private int treasureValue = 3;

    public override PickupType Type => PickupType.Gem;

    protected override void Collect(GameObject player)
    {
        GameManager.Instance.AddScore(PointValue);
        GameManager.Instance.AddTreasure(treasureValue);
        GameEvents.RaiseMessage("Gem collected! +" + treasureValue + " treasure");
    }
}
