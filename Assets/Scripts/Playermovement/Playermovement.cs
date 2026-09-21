using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Tweakable values — visible and editable in the Inspector too.
    public float moveSpeed = 6f;    // how fast the player walks
    public float jumpForce = 8f;    // how high the player jumps
    public float gravity = -20f;    // how strongly gravity pulls down

    // CharacterController handles movement + collision for us —
    // much simpler than manually working with Rigidbody in 3D.
    private CharacterController controller;

    // Tracks the player's current vertical speed (falling/jumping).
    private Vector3 velocity;

    void Start()
    {
        // Grab the CharacterController attached to this same GameObject.
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // isGrounded is built into CharacterController — true when
        // standing on something solid.
        bool isGrounded = controller.isGrounded;

        // If grounded and falling, reset vertical speed so we don't
        // build up infinite downward velocity while standing still.
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Read WASD / arrow keys.
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float moveZ = Input.GetAxis("Vertical");   // W/S or Up/Down

        // Build a movement direction relative to the world axes.
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Move the player horizontally.
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Jump: only allowed while grounded.
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Physics formula for jump height using gravity.
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        // Apply gravity over time (CharacterController doesn't do this
        // automatically like Rigidbody would).
        velocity.y += gravity * Time.deltaTime;

        // Apply the vertical movement (falling/jumping) separately.
        controller.Move(velocity * Time.deltaTime);
    }
    // Unity calls this automatically when the CharacterController's
    // collider overlaps with another collider marked "Is Trigger".
    // Note: in 3D this is OnTriggerEnter (no "2D" suffix like before).
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hazard"))
        {
            // Lose condition: send the player back to the start.
            Debug.Log("You Lose! Touched a hazard.");
            transform.position = new Vector3(0, 1, 0);
        }
        else if (other.CompareTag("Goal"))
        {
            // Win condition: log a message for now.
            Debug.Log("You Win! Reached the goal.");
        }
    }
    // A coroutine: code that can pause and resume over time.
    // Temporarily raises moveSpeed, waits, then lowers it back down.
    public System.Collections.IEnumerator SpeedBoost(float amount, float duration)
    {
        moveSpeed += amount;
        yield return new WaitForSeconds(duration);
        moveSpeed -= amount;
    }
}