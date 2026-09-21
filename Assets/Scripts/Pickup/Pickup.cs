using UnityEngine;

// This is our BASE class. Coin and PowerUp will both inherit from it.
// "abstract" means you can't put this script directly on an object by
// itself — it only exists to be built upon by other classes.
public abstract class Pickup : MonoBehaviour
{
    // ENCAPSULATION: pointValue is private, so no other script can
    // reach in and change it directly. [SerializeField] lets you still
    // edit it in the Inspector, but only through the controlled
    // property below.
    [SerializeField] private int pointValue = 1;

    // A public "window" into the private field — read-only from outside.
    public int PointValue => pointValue;

    // Unity calls this automatically when something touches this
    // object's trigger collider.
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect(other.gameObject);
            Destroy(gameObject); // pickup disappears once collected
        }
    }

    // POLYMORPHISM: this method has NO body here — it's just a promise
    // that every subclass (Coin, PowerUp) must fill in its own version.
    // When Collect() runs, C# automatically picks the correct version
    // based on what type of pickup it actually is.
    protected abstract void Collect(GameObject player);
}