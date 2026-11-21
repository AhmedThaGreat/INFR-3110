using UnityEngine;

// Keeps the beach stocked with treasure.
// Every few seconds, if there are fewer pickups out than the max, it picks a
// random spot on the ground and asks the FACTORY for a pickup there.
// It doesn't know about prefabs or pools - that's the factory's job.
public class PickupSpawner : MonoBehaviour
{
    public PickupFactory factory;
    public Transform player;

    [Header("Where pickups can appear (a box centred on this object)")]
    public Vector2 areaSize = new Vector2(40f, 40f);
    public float heightAboveGround = 1f;
    public float minGroundHeight = -100f;   // ground lower than this (under the sea) is skipped

    [Header("How many, how fast")]
    public int maxActivePickups = 12;
    public float respawnDelay = 2f;

    [Header("Keep pickups apart")]
    public float minDistanceFromPlayer = 4f;
    public float minDistanceBetweenPickups = 2.5f;

    private float timer;

    void Start()
    {
        if (factory == null) factory = FindAnyObjectByType<PickupFactory>();
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }

        // Fill the level straight away so it isn't empty at the start.
        for (int i = 0; i < maxActivePickups; i++)
        {
            SpawnOne();
        }
    }

    void Update()
    {
        if (factory == null) return;
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

        if (factory.ActiveCount < maxActivePickups)
        {
            timer += Time.deltaTime;
            if (timer >= respawnDelay)
            {
                timer = 0f;
                SpawnOne();
            }
        }
    }

    // Try a few random spots and use the first good one.
    void SpawnOne()
    {
        if (factory == null) return;

        for (int attempt = 0; attempt < 15; attempt++)
        {
            Vector3 point;
            if (FindGroundPoint(out point) && IsSpotFree(point))
            {
                factory.CreateRandomPickup(point + Vector3.up * heightAboveGround);
                return;
            }
        }
    }

    // Shoot a ray straight down from high above a random spot to find the sand.
    bool FindGroundPoint(out Vector3 point)
    {
        float x = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);
        float z = Random.Range(-areaSize.y / 2f, areaSize.y / 2f);
        Vector3 from = transform.position + new Vector3(x, 50f, z);

        RaycastHit hit;
        if (Physics.Raycast(from, Vector3.down, out hit, 200f, ~0, QueryTriggerInteraction.Ignore))
        {
            // Don't put treasure on top of the player, hazards or the water.
            if (!hit.collider.CompareTag("Player") && !hit.collider.CompareTag("Hazard")
                && hit.collider.gameObject.layer != 4 && hit.point.y >= minGroundHeight)
            {
                point = hit.point;
                return true;
            }
        }

        point = Vector3.zero;
        return false;
    }

    bool IsSpotFree(Vector3 point)
    {
        if (player != null && Vector3.Distance(point, player.position) < minDistanceFromPlayer) return false;

        // Look for other pickups (they use trigger colliders) close by.
        Collider[] nearby = Physics.OverlapSphere(point + Vector3.up * heightAboveGround,
                                                  minDistanceBetweenPickups, ~0, QueryTriggerInteraction.Collide);
        foreach (Collider c in nearby)
        {
            if (c.GetComponentInParent<Pickup>() != null) return false;
            if (c.CompareTag("Hazard") || c.CompareTag("Goal")) return false;
        }
        return true;
    }

    // Draws the spawn area in the Scene view when this object is selected.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.4f);
        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, 1f, areaSize.y));
    }
}
