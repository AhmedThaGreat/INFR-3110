using UnityEngine;

// Makes a pickup slowly spin and float up and down so it catches the eye.
public class Spinner : MonoBehaviour
{
    public float spinSpeed = 90f;   // degrees per second
    public float bobHeight = 0.2f;  // how far it floats up and down
    public float bobSpeed = 2f;     // how fast it floats

    private Vector3 startPos;
    private float timeOffset;

    // OnEnable instead of Start: a pooled pickup is turned on again at a NEW
    // spot, so it has to remember its new starting point every time.
    void OnEnable()
    {
        startPos = transform.position;
        timeOffset = Random.Range(0f, 10f);   // so pickups don't all bob in sync
    }

    void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        transform.position = startPos + Vector3.up * Mathf.Sin((Time.time + timeOffset) * bobSpeed) * bobHeight;
    }
}
