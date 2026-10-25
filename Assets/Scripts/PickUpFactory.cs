using UnityEngine;

// An "enum" is a labeled list of options, so we can say
// "make a Gem" using plain words.
public enum PickupType
{
    Coin,
    PowerUp,
    Gem
}

public class PickupFactory : MonoBehaviour
{
    // Drag the PREFABS (the blueprints) into these slots in the Inspector.
    public GameObject coinPrefab;
    public GameObject powerUpPrefab;
    public GameObject gemPrefab;

    // The factory method: say what you want and where, and it picks the
    // right prefab. The calling code never needs to know the details.
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
            case PickupType.Gem:
                prefabToSpawn = gemPrefab;
                break;
        }

        if (prefabToSpawn != null)
        {
            return Instantiate(prefabToSpawn, position, Quaternion.identity);
        }
        return null;
    }

    // DEMO: spawn a few pickups at the start so you can see it working.
    void Start()
    {
        CreatePickup(PickupType.Coin, new Vector3(-4, 1, 4));
        CreatePickup(PickupType.Coin, new Vector3(-2, 1, 4));
        CreatePickup(PickupType.PowerUp, new Vector3(0, 1, 4));
        CreatePickup(PickupType.Gem, new Vector3(2, 1, 4));
    }
}