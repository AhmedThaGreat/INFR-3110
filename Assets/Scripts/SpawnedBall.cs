using UnityEngine;

// One cannonball.
// Flowchart: fly through the air -> did I hit the player? -> count up my age
//            -> when my age reaches my lifetime, tell the cannon "I'm finished".
public class SpawnedBall : MonoBehaviour
{
    public float lifetime = 2f;       // how many seconds the ball lives
    public float hitRadius = 0.8f;    // how close counts as a hit

    private float age = 0f;
    private Vector3 velocity;
    private ObjectSpawner spawner;    // the cannon that fired this ball
    private Transform target;         // the player (null = this ball can't hurt anyone)
    private int damage = 1;
    private bool finished = false;

    // The cannon calls this every time it fires (or reuses) the ball.
    // Resetting everything here is what lets a reused ball start fresh.
    public void Launch(Vector3 startVelocity, ObjectSpawner owner, Transform hurtTarget, int hitDamage)
    {
        velocity = startVelocity;
        spawner = owner;
        target = hurtTarget;
        damage = hitDamage;
        age = 0f;
        finished = false;
    }

    void Update()
    {
        if (finished) return;

        // Step 1: move the ball (gravity pulls the speed down each frame)
        velocity = velocity + Physics.gravity * Time.deltaTime;
        transform.position = transform.position + velocity * Time.deltaTime;

        // Step 2: did it reach the player? (a distance check is cheaper than physics)
        if (target != null)
        {
            Vector3 chest = target.position + Vector3.up;
            if ((transform.position - chest).sqrMagnitude < hitRadius * hitRadius)
            {
                GameManager.Instance.DamagePlayer(damage, transform.position);
                Finish();
                return;
            }
        }

        // Step 3: get older, and finish when the lifetime is up
        age = age + Time.deltaTime;
        if (age >= lifetime)
        {
            Finish();
        }
    }

    void Finish()
    {
        finished = true;
        if (spawner != null)
        {
            spawner.BallFinished(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
