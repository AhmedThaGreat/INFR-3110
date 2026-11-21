using UnityEngine;

// A third-person camera that follows the player.
// Hold the RIGHT mouse button and move the mouse to look around,
// scroll the wheel to zoom.
// Put this on the Main Camera. It unparents itself from the player on start,
// so the camera doesn't spin when the character turns.
public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;              // the player (found automatically by tag)
    public float distance = 7f;
    public float minDistance = 3f;
    public float maxDistance = 12f;
    public float pivotHeight = 1.8f;      // look at the head, not the feet
    public float mouseSensitivity = 3f;
    public float minPitch = -5f;
    public float maxPitch = 60f;
    public float followSmoothness = 12f;
    public bool holdRightMouseToLook = true;

    private float yaw;
    private float pitch = 18f;

    void Awake()
    {
        transform.SetParent(null);

        if (target == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) target = p.transform;
        }
        if (target != null) yaw = target.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (!holdRightMouseToLook || Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        }
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        distance = Mathf.Clamp(distance - Input.mouseScrollDelta.y * 0.5f, minDistance, maxDistance);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 pivot = target.position + Vector3.up * pivotHeight;
        Vector3 wanted = pivot - rotation * Vector3.forward * distance;

        // Don't let palm trees or rocks get between the camera and the player.
        RaycastHit hit;
        Vector3 toCamera = wanted - pivot;
        if (Physics.SphereCast(pivot, 0.25f, toCamera.normalized, out hit, toCamera.magnitude,
                               ~0, QueryTriggerInteraction.Ignore)
            && hit.transform != target && !hit.transform.IsChildOf(target))
        {
            wanted = pivot + toCamera.normalized * Mathf.Max(hit.distance - 0.2f, 0.5f);
        }

        float t = 1f - Mathf.Exp(-followSmoothness * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, wanted, t);
        transform.rotation = Quaternion.LookRotation(pivot - transform.position);
    }
}
