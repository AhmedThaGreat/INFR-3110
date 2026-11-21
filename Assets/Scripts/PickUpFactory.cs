using System.Collections.Generic;
using UnityEngine;

// An "enum" is a labeled list of options, so we can say "make a Gem" in plain words.
public enum PickupType
{
    Coin,
    PowerUp,
    Gem
}

// FACTORY PATTERN (upgraded since the check-in).
//
// Before: the factory only made 4 pickups at fixed spots when the game started.
// Now:    it is the ONE place that decides what kind of pickup appears and where
//         it comes from, for the whole game:
//           - CreateRandomPickup() rolls a weighted dice (coins common, gems rare)
//           - it takes the object from that type's OBJECT POOL instead of
//             Instantiate, and takes it back when it's collected
//         The PickupSpawner just asks "give me a pickup here" and never needs
//         to know about prefabs, odds or pools.
public class PickupFactory : MonoBehaviour
{
    [Header("Prefabs (the blueprints)")]
    public GameObject coinPrefab;
    public GameObject powerUpPrefab;
    public GameObject gemPrefab;

    [Header("How often each type appears (higher = more common)")]
    public int coinWeight = 75;
    public int gemWeight = 15;
    public int powerUpWeight = 10;

    [Header("Pool size per type")]
    public int maxPerType = 30;

    // One pool for each type of pickup.
    private Dictionary<PickupType, SimplePool<Pickup>> pools = new Dictionary<PickupType, SimplePool<Pickup>>();

    void Awake()
    {
        Transform holder = new GameObject("Pooled Pickups").transform;

        AddPool(PickupType.Coin, coinPrefab, holder);
        AddPool(PickupType.Gem, gemPrefab, holder);
        AddPool(PickupType.PowerUp, powerUpPrefab, holder);
    }

    void AddPool(PickupType type, GameObject prefab, Transform holder)
    {
        if (prefab == null) return;

        Pickup pickupScript = prefab.GetComponent<Pickup>();
        if (pickupScript == null)
        {
            Debug.LogWarning(prefab.name + " has no Pickup script, so the factory can't pool it.");
            return;
        }
        pools[type] = new SimplePool<Pickup>(pickupScript, holder, maxPerType);
    }

    // The factory method: say what you want and where.
    public Pickup CreatePickup(PickupType type, Vector3 position)
    {
        if (!pools.ContainsKey(type)) return null;

        Pickup pickup = pools[type].Get(position, Quaternion.identity);
        if (pickup != null)
        {
            pickup.Owner = this;   // so it comes back to this factory when collected
        }
        return pickup;
    }

    // Same, but the factory also decides WHICH type, using the weights.
    public Pickup CreateRandomPickup(Vector3 position)
    {
        return CreatePickup(PickRandomType(), position);
    }

    // Weighted dice roll, e.g. 75 / 15 / 10 out of 100.
    public PickupType PickRandomType()
    {
        int total = coinWeight + gemWeight + powerUpWeight;
        int roll = Random.Range(0, total);

        if (roll < coinWeight) return PickupType.Coin;
        if (roll < coinWeight + gemWeight) return PickupType.Gem;
        return PickupType.PowerUp;
    }

    // A collected pickup comes back here: turned off, not destroyed.
    public void ReturnPickup(Pickup pickup)
    {
        if (pools.ContainsKey(pickup.Type))
        {
            pools[pickup.Type].Release(pickup);
        }
        else
        {
            pickup.gameObject.SetActive(false);
        }
    }

    // How many pickups are out in the level right now.
    public int ActiveCount
    {
        get
        {
            int count = 0;
            foreach (SimplePool<Pickup> pool in pools.Values) count += pool.CountActive;
            return count;
        }
    }

    // How many pickup objects were ever really created (for profiling).
    public int TotalCreated
    {
        get
        {
            int count = 0;
            foreach (SimplePool<Pickup> pool in pools.Values) count += pool.TotalCreated;
            return count;
        }
    }
}
