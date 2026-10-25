using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Tweakable values, visible and editable in the Inspector too.
    public float moveSpeed = 6f;    // how fast the player walks
    public float jumpForce = 8f;    // how high the player jumps
    public float gravity = -20f;    // how strongly gravity pulls down

    // CharacterController handles movement + collision for us.
    private CharacterController controller;

    // Tracks the player's current vertical speed (falling/jumping).
    private Vector3 velocity;

    // The direction the player was told to walk this frame.
    // Commands add to this; Update() uses it, then clears it.
    private Vector3 moveInput;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Called by a MoveCommand. It ADDS a direction, so holding W and D
    // together gives a diagonal move.
    public void Move(float x, float z)
    {
        moveInput += transform.right * x + transform.forward * z;
    }

    // Called by a JumpCommand. Only works while standing on the ground.
    public void Jump()
    {
        if (controller.isGrounded)
        {
            // Physics formula for jump height using gravity.
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    // Teleports the player back to the start. A CharacterController ignores
    // normal position changes, so we switch it off for a moment, move the
    // player, then switch it back on.
    public void Respawn()
    {
        controller.enabled = false;
        transform.position = new Vector3(0f, 1f, 0f);
        velocity = Vector3.zero;     // stop any falling or jumping speed
        controller.enabled = true;
    }


    void Update()
    {
        // If grounded and falling, reset vertical speed so it
        // doesn't build up while standing still.
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Limit the length to 1 so diagonal movement isn't faster.
        Vector3 move = Vector3.ClampMagnitude(moveInput, 1f);
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Clear it, so the player stops when no keys are held.
        moveInput = Vector3.zero;

        // Apply gravity over time.
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // Unity calls this when the CharacterController overlaps a trigger collider.
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hazard"))
        {
            GameManager.Instance.Lose();
            Respawn();
        }
        else if (other.CompareTag("Goal"))
        {
            GameManager.Instance.Win();
        }
    }

    // A coroutine: temporarily raises moveSpeed, waits, then lowers it back.
    public System.Collections.IEnumerator SpeedBoost(float amount, float duration)
    {
        moveSpeed += amount;
        yield return new WaitForSeconds(duration);
        moveSpeed -= amount;
    }
}