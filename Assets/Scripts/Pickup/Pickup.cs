using UnityEngine;

// BASE class for every pickup. Coin, Gem and PowerUp inherit from it.
// "abstract" means it only exists to be built upon.
public abstract class Pickup : MonoBehaviour
{
    // ENCAPSULATION: private, but editable in the Inspector.
    [SerializeField] private int pointValue = 1;
    public int PointValue => pointValue;

    // Each subclass says what kind of pickup it is (used by the factory's pools
    // and by the PickupCollected event).
    public abstract PickupType Type { get; }

    // The factory that made this pickup. If it came from a factory it goes back
    // to that factory's pool instead of being destroyed.
    public PickupFactory Owner { get; set; }

    private bool collected;

    // A pooled pickup is turned on again when it is reused, so reset it here.
    private void OnEnable()
    {
        collected = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;                    // stops double pick-ups in one frame
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

        collected = true;
        Collect(other.gameObject);

        // Observer: announce it. Sound, achievements and the HUD react on their own.
        GameEvents.RaisePickupCollected(Type, transform.position);

        if (Owner != null)
        {
            Owner.ReturnPickup(this);   // back into the pool, ready to respawn
        }
        else
        {
            Destroy(gameObject);        // a pickup placed by hand in the scene
        }
    }

    // POLYMORPHISM: every subclass fills in its own version.
    protected abstract void Collect(GameObject player);
}
