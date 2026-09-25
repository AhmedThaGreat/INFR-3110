using UnityEngine;

// A GENERIC base class — the <T> means "this works for any class type."
// Any class that inherits from Singleton<ItsOwnName> automatically gets
// the "there's only ever one, and everyone can reach it" behavior for free.
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // This holds THE one-and-only instance. It's private, so nothing
    // outside this class can directly overwrite it.
    private static T instance;

    // The public "front door" everyone uses to reach the single instance.
    // Example usage elsewhere: GameManager.Instance.AddScore(1);
    public static T Instance
    {
        get
        {
            // If nobody has grabbed the instance yet, find it in the scene.
            if (instance == null)
            {
                instance = FindFirstObjectByType<T>();
            }
            return instance;
        }
    }

    // Runs automatically when this object is created in the scene.
    protected virtual void Awake()
    {
        if (instance == null)
        {
            // First one created — this becomes THE instance.
            instance = this as T;
        }
        else if (instance != this)
        {
            // A second copy accidentally exists — destroy the extra one
            // so we never end up with two "principals" by mistake.
            Destroy(gameObject);
        }
    }
}
