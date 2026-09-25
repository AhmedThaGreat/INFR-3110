using UnityEngine;

// An "enum" is just a labeled list of options. This lets us say
// "make a Coin" or "make a PowerUp" using plain words instead of
// numbers or strings.
public enum PickupType
{
    Coin,
    PowerUp
}

public class PickupFactory : MonoBehaviour
{
    // Drag your Coin and PowerUp PREFABS (not scene objects) into
    // these slots in the Inspector — these are the "blueprints."
    public GameObject coinPrefab;
    public GameObject powerUpPrefab;

    // THIS is the actual factory method. You tell it what type you
    // want and where, and it figures out which prefab to build —
    // the calling code never needs to know the details.
    public GameObject CreatePickup(PickupType type, Vector3 position)
    {
        GameObject prefabToSpawn = null;

        switch (type)
        {
            case PickupType.Coin:
                prefabToSpawn = coinPrefab;
                break;
            case PickupType.PowerUp:
                prefabToSpawn = powerUpPrefab;
                break;
        }

        if (prefabToSpawn != null)
        {
            return Instantiate(prefabToSpawn, position, Quaternion.identity);
        }
        return null;
    }

    // DEMO: as soon as the game starts, use the factory to spawn a
    // few pickups automatically — this is just so you can SEE it
    // working without extra setup.
    void Start()
    {
        CreatePickup(PickupType.Coin, new Vector3(-4, 1, 4));
        CreatePickup(PickupType.Coin, new Vector3(-2, 1, 4));
        CreatePickup(PickupType.PowerUp, new Vector3(0, 1, 4));
    }
}