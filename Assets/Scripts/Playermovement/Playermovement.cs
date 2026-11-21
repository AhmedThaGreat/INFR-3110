using UnityEngine;

// Moves the player. It never reads the keyboard itself: the InputHandler runs
// MoveCommand / JumpCommand objects, and those call Move() and Jump() here.
//
// What changed since the check-in:
//  - Movement is now relative to the CAMERA, and the character turns to face
//    the way it walks (needed for a real character model).
//  - The player's STATE MACHINE can switch movement off (CanMove) while the
//    player is hurt, dead or celebrating.
//  - Hazards now cost a heart instead of instantly losing the game.
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 6f;    // how fast the player walks
    public float jumpForce = 8f;    // how high the player jumps
    public float gravity = -20f;    // how strongly gravity pulls down
    public float turnSpeed = 12f;   // how fast the character turns to face its direction
    public Transform cameraTransform; // leave empty to use the Main Camera
    public float fallLimit = -15f;  // falling below this height costs a heart and respawns

    // The state machine turns this off while hurt / dead / celebrating.
    public bool CanMove { get; set; } = true;

    // Read by the state machine to pick Idle / Run / Jump.
    public bool IsGrounded => controller != null && controller.isGrounded;
    public float HorizontalSpeed { get; private set; }
    public float VerticalVelocity => velocity.y;
    public float TimeSinceGrounded { get; private set; }

    private CharacterController controller;
    private Vector3 velocity;     // vertical speed (falling / jumping)
    private Vector3 heldDirection; // the sum of all walking keys currently held (set by commands)
    private Vector3 knockback;    // push from being hit, fades out quickly
    private Vector3 spawnPoint;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        spawnPoint = transform.position;
    }

    // Called ONCE by a MoveCommand when a key goes down (and once with the
    // opposite values by Undo() when it comes up). x = left/right, z = forward/back.
    // It ADDS, so holding W and D together gives a diagonal.
    public void AddMoveDirection(float x, float z)
    {
        heldDirection += new Vector3(x, 0f, z);
    }

    // Called by a JumpCommand. Only works while standing on the ground.
    public void Jump()
    {
        if (CanMove && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);   // jump height from gravity
        }
    }

    // Called by the Hurt state: shove the player away from whatever hit them.
    public void Knockback(Vector3 hitFrom, float force)
    {
        Vector3 away = transform.position - hitFrom;
        away.y = 0f;
        if (away.sqrMagnitude < 0.01f) away = -transform.forward;
        knockback = away.normalized * force;
        velocity.y = Mathf.Max(velocity.y, 4f);   // a little hop
    }

    // Teleport back to the start. A CharacterController ignores normal position
    // changes, so switch it off for a moment.
    public void Respawn()
    {
        controller.enabled = false;
        transform.position = spawnPoint;
        velocity = Vector3.zero;
        knockback = Vector3.zero;
        controller.enabled = true;
    }

    void Update()
    {
        if (controller.isGrounded)
        {
            TimeSinceGrounded = 0f;
            if (velocity.y < 0f) velocity.y = -2f;   // keep it pressed to the ground
        }
        else
        {
            TimeSinceGrounded += Time.deltaTime;
        }

        // Turn the key input into a world direction based on where the camera looks.
        Vector3 input = Vector3.ClampMagnitude(heldDirection, 1f);

        Vector3 worldMove = Vector3.zero;
        if (CanMove && input.sqrMagnitude > 0.01f)
        {
            Transform cam = cameraTransform != null ? cameraTransform
                          : (Camera.main != null ? Camera.main.transform : null);

            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.right;
            if (cam != null)
            {
                forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
                right = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
            }
            worldMove = right * input.x + forward * input.z;

            // Face the direction we're walking.
            Quaternion facing = Quaternion.LookRotation(worldMove);
            transform.rotation = Quaternion.Slerp(transform.rotation, facing, turnSpeed * Time.deltaTime);
        }

        HorizontalSpeed = worldMove.magnitude * moveSpeed;

        // The knockback push fades out over about half a second.
        knockback = Vector3.MoveTowards(knockback, Vector3.zero, 12f * Time.deltaTime);

        controller.Move((worldMove * moveSpeed + knockback) * Time.deltaTime);

        // Gravity.
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Fell off the island.
        if (transform.position.y < fallLimit)
        {
            if (GameManager.Instance != null) GameManager.Instance.DamagePlayer(1, transform.position);
            Respawn();
        }
    }

    // Unity calls this when the CharacterController overlaps a trigger collider.
    void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance == null) return;

        if (other.CompareTag("Hazard"))
        {
            GameManager.Instance.DamagePlayer(1, other.transform.position);
        }
        else if (other.CompareTag("Goal"))
        {
            GameManager.Instance.TryWin();
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
