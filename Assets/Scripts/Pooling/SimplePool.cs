using System.Collections.Generic;
using UnityEngine;

// OBJECT POOL (optimization pattern) - a reusable version of the pool I built
// for the profiling lab, so the same idea now serves two jobs:
//   1. the cannon's cannonballs
//   2. the factory's coins, gems and power-ups
//
// Flowchart for Get():
//   look through the list for an object that is turned off
//     found one  -> move it into place, turn it on, hand it back
//     none found -> is the pool smaller than the max?
//                     yes -> create a new one, add it to the list
//                     no  -> return nothing (pool is full)
//
// Objects are never destroyed. Release() just turns them off so they can be
// reused, which means no new memory and no garbage collector spikes.
public class SimplePool<T> where T : Component
{
    private readonly T prefab;          // the blueprint to copy
    private readonly Transform parent;  // keeps the Hierarchy tidy
    private readonly int maxSize;       // the most objects this pool may ever hold
    private readonly List<T> items = new List<T>();

    public int TotalCreated { get; private set; }   // how many were really created
    public int Size => items.Count;

    public int CountActive
    {
        get
        {
            int count = 0;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null && items[i].gameObject.activeSelf) count++;
            }
            return count;
        }
    }

    public SimplePool(T prefab, Transform parent, int maxSize)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.maxSize = maxSize;
    }

    // Hand out an object at this position, or null if the pool is full.
    public T Get(Vector3 position, Quaternion rotation)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null && items[i].gameObject.activeSelf == false)
            {
                // Move it BEFORE turning it on, so its OnEnable sees the new spot.
                items[i].transform.SetPositionAndRotation(position, rotation);
                items[i].gameObject.SetActive(true);
                return items[i];
            }
        }

        if (items.Count < maxSize)
        {
            T newItem = Object.Instantiate(prefab, position, rotation, parent);
            items.Add(newItem);
            TotalCreated = TotalCreated + 1;
            return newItem;
        }

        return null;
    }

    // Put an object back: turned off, NOT destroyed.
    public void Release(T item)
    {
        if (item != null)
        {
            item.gameObject.SetActive(false);
        }
    }

    // Optional: create some objects up front (during loading) instead of mid-game.
    public void Prewarm(int count)
    {
        while (items.Count < count && items.Count < maxSize)
        {
            T newItem = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity, parent);
            newItem.gameObject.SetActive(false);
            items.Add(newItem);
            TotalCreated = TotalCreated + 1;
        }
    }
}
