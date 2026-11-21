using UnityEngine;

// Also inherits from Pickup - same foundation, different behaviour.
public class PowerUp : Pickup
{
    public float speedBoostAmount = 3f;
    public float boostDuration = 5f;

    public override PickupType Type => PickupType.PowerUp;

    // POLYMORPHISM: one method name (Collect), a different behaviour.
    protected override void Collect(GameObject player)
    {
        GameEvents.RaiseMessage("Power-up! Speed boost for " + boostDuration.ToString("0") + " seconds");

        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.StartCoroutine(movement.SpeedBoost(speedBoostAmount, boostDuration));
        }
    }
}
