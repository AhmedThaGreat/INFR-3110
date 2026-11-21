using UnityEngine;

// What the cannon is being used for.
public enum CannonMode
{
    Gameplay,    // a real hazard: fires every few seconds when the player is in range
    StressTest   // fires hundreds per second, only for the profiler comparison
}

// The pirate CANNON. It aims at the player and fires cannonballs.
// A cannonball that reaches the player costs a heart.
//
// OBJECT POOL: with "Use Pooling" ticked, cannonballs come from a SimplePool
// and are turned off (not destroyed) when they land. Unticked, every ball is
// made with Instantiate() and removed with Destroy() - kept on purpose so the
// profiler can compare the two.
public class ObjectSpawner : MonoBehaviour
{
    [Header("Cannon")]
    public SpawnedBall ballPrefab;      // drag the Ball PREFAB here
    public Transform target;            // drag the PLAYER here
    public CannonMode mode = CannonMode.Gameplay;
    public float lifetime = 2.5f;       // seconds each ball lives (more than Flight Time)
    public float flightTime = 1.2f;     // seconds a ball takes to reach the player
    public float aimSpread = 0.5f;      // random miss so the player can dodge
    public float muzzleOffset = 1.6f;   // how far in front of the cannon balls appear

    [Header("Gameplay mode")]
    public float fireInterval = 2.5f;   // seconds between shots
    public float range = 25f;           // only fires when the player is this close
    public int damage = 1;

    [Header("Stress test mode (profiling only)")]
    public float spawnsPerSecond = 400f;

    [Header("Object pooling")]
    public bool usePooling = true;      // OFF for the "before", ON for the "after"
    public int maxPoolSize = 500;       // the most balls the pool may hold

    [Header("Live numbers (just look, don't edit)")]
    [SerializeField] private int activeBalls = 0;
    [SerializeField] private int totalCreated = 0;

    private SimplePool<SpawnedBall> pool;
    private Transform ballHolder;
    private float spawnTimer = 0f;

    void Start()
    {
        // Each cannon gets its own pool, so every cannon's balls are grouped under its name.
        ballHolder = new GameObject(name + " Balls").transform;
        pool = new SimplePool<SpawnedBall>(ballPrefab, ballHolder, maxPoolSize);

        // With several cannons, start each one at a random point in its timer
        // so they don't all fire at exactly the same moment.
        spawnTimer = Random.Range(0f, fireInterval);

        if (target == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) target = p.transform;
        }
    }

    void Update()
    {
        if (target == null || ballPrefab == null) return;
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

        // Step 1: turn the cannon to face the player
        Vector3 aimPoint = target.position + Vector3.up;
        Vector3 direction = aimPoint - transform.position;
        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(direction), 6f * Time.deltaTime);
        }

        // Step 2: is it time to fire?
        if (mode == CannonMode.Gameplay)
        {
            if (direction.magnitude > range) return;   // player is too far away

            spawnTimer += Time.deltaTime;
            if (spawnTimer >= fireInterval)
            {
                spawnTimer = 0f;
                FireBall(true);
                GameEvents.RaiseCannonFired(transform.position);
            }
        }
        else
        {
            // Stress test: many balls per frame, they can't hurt the player.
            float timeBetweenShots = 1f / spawnsPerSecond;
            spawnTimer += Time.deltaTime;

            int shotsThisFrame = 0;
            while (spawnTimer >= timeBetweenShots && shotsThisFrame < 50)
            {
                spawnTimer -= timeBetweenShots;
                FireBall(false);
                shotsThisFrame++;
            }
        }
    }

    // pooling on?  yes -> ask the pool for a ball
    //              no  -> make a brand new ball
    // then: set the start position -> work out the speed -> launch it
    void FireBall(bool canHurtPlayer)
    {
        Vector3 startPosition = transform.position + transform.forward * muzzleOffset;
        SpawnedBall ball;

        if (usePooling)
        {
            ball = pool.Get(startPosition, Quaternion.identity);
            if (ball == null) return;   // pool is full and every ball is busy
            totalCreated = pool.TotalCreated;
        }
        else
        {
            ball = Instantiate(ballPrefab, startPosition, Quaternion.identity, ballHolder);
            totalCreated++;
        }

        // The speed that lands the ball on the player after flightTime seconds
        // (the second part makes up for gravity pulling it down).
        Vector3 aimPoint = target.position + Vector3.up + Random.insideUnitSphere * aimSpread;
        Vector3 startVelocity = (aimPoint - startPosition) / flightTime
                                - 0.5f * Physics.gravity * flightTime;

        ball.lifetime = lifetime;
        ball.Launch(startVelocity, this, canHurtPlayer ? target : null, damage);
        activeBalls++;
    }

    // A ball calls this when its time is up or it hits the player.
    //   pooling on  -> turn it off so it can be reused (NOT destroyed)
    //   pooling off -> destroy it
    public void BallFinished(SpawnedBall ball)
    {
        activeBalls--;

        if (usePooling)
        {
            pool.Release(ball);
        }
        else
        {
            Destroy(ball.gameObject);
        }
    }
}
