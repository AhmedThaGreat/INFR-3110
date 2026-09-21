using UnityEngine;

// Also inherits from Pickup — same foundation, different behaviour.
public class PowerUp : Pickup
{
    public float speedBoostAmount = 3f;
    public float boostDuration = 5f;

    // A DIFFERENT override of the same Collect() method — this is
    // exactly what polymorphism means: one method name, many behaviours.
    protected override void Collect(GameObject player)
    {
        Debug.Log("Collected a power-up! Speed boosted temporarily.");
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.StartCoroutine(pm.SpeedBoost(speedBoostAmount, boostDuration));
        }
    }
}
