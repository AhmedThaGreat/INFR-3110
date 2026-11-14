using System.Collections.Generic;
using UnityEngine;

// A cannon that fires balls at the player.
// Tick "Use Pooling" in the Inspector to switch between:
//   OFF = Instantiate() and Destroy()   (the "before")
//   ON  = object pool                   (the "after")
public class ObjectSpawner : MonoBehaviour
{
    [Header("Cannon")]
    public SpawnedBall ballPrefab;      // drag the Ball PREFAB here
    public Transform target;            // drag the PLAYER here
    public float spawnsPerSecond = 120f;
    public float lifetime = 2f;         // seconds each ball lives (more than Flight Time)
    public float flightTime = 1.2f;     // seconds a ball takes to reach the player
    public float aimSpread = 0.5f;      // random miss so the balls form a stream
    public float muzzleOffset = 1.6f;   // how far in front of the cannon the balls appear

    [Header("Object pooling")]
    public bool usePooling = false;     // OFF for the "before", ON for the "after"
    public int maxPoolSize = 500;       // the most balls the pool is allowed to hold

    [Header("Live numbers (just look, don't edit)")]
    [SerializeField] private int activeBalls = 0;    // balls in the air right now
    [SerializeField] private int totalCreated = 0;   // how many balls were really created

    private List<SpawnedBall> pool = new List<SpawnedBall>();   // every ball the pool owns
    private float spawnTimer = 0f;
    private Transform ballHolder;       // an empty object that keeps the Hierarchy tidy

    void Start()
    {
        ballHolder = new GameObject("Spawned Balls").transform;
    }

    void Update()
    {
        // Step 1: turn the cannon to face the player
        if (target != null)
        {
            Vector3 aimPoint = target.position + Vector3.up;
            Vector3 direction = aimPoint - transform.position;
            if (direction.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        // Step 2: work out if it is time to fire
        float timeBetweenShots = 1f / spawnsPerSecond;
        spawnTimer = spawnTimer + Time.deltaTime;

        int shotsThisFrame = 0;
        while (spawnTimer >= timeBetweenShots && shotsThisFrame < 50)
        {
            spawnTimer = spawnTimer - timeBetweenShots;
            FireBall();
            shotsThisFrame = shotsThisFrame + 1;
        }
    }

    // Flowchart for firing one ball:
    // pooling on?  yes -> ask the pool for a ball
    //              no  -> make a brand new ball
    // then: set the start position -> work out the speed -> launch it
    void FireBall()
    {
        if (target == null)
        {
            return;   // nothing to aim at (drag the Player into the Target slot)
        }

        SpawnedBall ball;

        if (usePooling == true)
        {
            ball = GetBallFromPool();
            if (ball == null)
            {
                return;   // pool is full and every ball is busy, so skip this shot
            }
        }
        else
        {
            ball = Instantiate(ballPrefab, ballHolder);
            totalCreated = totalCreated + 1;
        }

        // Start at the end of the cannon
        Vector3 startPosition = transform.position + transform.forward * muzzleOffset;
        ball.transform.position = startPosition;

        // Work out the speed that makes the ball reach the player after flightTime
        // seconds (the second part makes up for gravity pulling it down)
        Vector3 aimPoint = target.position + Vector3.up + Random.insideUnitSphere * aimSpread;
        Vector3 startVelocity = (aimPoint - startPosition) / flightTime
                                - 0.5f * Physics.gravity * flightTime;

        ball.lifetime = lifetime;
        ball.Launch(startVelocity, this);
        activeBalls = activeBalls + 1;
    }

    // Flowchart for the pool:
    // look through the list for a ball that is turned off
    //   found one      -> turn it on and use it
    //   none found     -> is the pool smaller than the max?
    //                       yes -> create a new ball and add it to the list
    //                       no  -> return nothing (pool is full)
    SpawnedBall GetBallFromPool()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i].gameObject.activeSelf == false)
            {
                pool[i].gameObject.SetActive(true);
                return pool[i];
            }
        }

        if (pool.Count < maxPoolSize)
        {
            SpawnedBall newBall = Instantiate(ballPrefab, ballHolder);
            totalCreated = totalCreated + 1;
            pool.Add(newBall);
            return newBall;
        }

        return null;
    }

    // A ball calls this when its lifetime is up.
    //   pooling on  -> turn the ball off so it can be reused (NOT destroyed)
    //   pooling off -> destroy the ball
    public void BallFinished(SpawnedBall ball)
    {
        activeBalls = activeBalls - 1;

        if (usePooling == true)
        {
            ball.gameObject.SetActive(false);
        }
        else
        {
            Destroy(ball.gameObject);
        }
    }
}
