using UnityEngine;

// One cannonball.
// Flowchart: fly through the air -> count up its age -> when the age reaches
// the lifetime, tell the spawner "I'm finished".
public class SpawnedBall : MonoBehaviour
{
    public float lifetime = 2f;       // how many seconds the ball lives

    private float age = 0f;           // how long the ball has been alive
    private Vector3 velocity;         // which way (and how fast) it is moving
    private ObjectSpawner spawner;    // the cannon that fired this ball

    // The spawner calls this every time it fires (or reuses) the ball.
    // Setting age back to 0 here is what lets a reused ball start fresh.
    public void Launch(Vector3 startVelocity, ObjectSpawner owner)
    {
        velocity = startVelocity;
        spawner = owner;
        age = 0f;
    }

    void Update()
    {
        // Step 1: move the ball (gravity pulls the speed down each frame)
        velocity = velocity + Physics.gravity * Time.deltaTime;
        transform.position = transform.position + velocity * Time.deltaTime;

        // Step 2: get older
        age = age + Time.deltaTime;

        // Step 3: is the lifetime up? If yes, tell the spawner.
        if (age >= lifetime)
        {
            spawner.BallFinished(this);
        }
    }
}