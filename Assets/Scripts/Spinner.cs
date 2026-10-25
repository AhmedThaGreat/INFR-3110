using UnityEngine;

// Makes a pickup slowly spin and float up and down so it catches the eye.
public class Spinner : MonoBehaviour
{
    public float spinSpeed = 90f;   // degrees per second
    public float bobHeight = 0.2f;  // how far it floats up and down
    public float bobSpeed = 2f;     // how fast it floats

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Spin around the vertical axis.
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);

        // Float gently up and down around the start position.
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
    }
}