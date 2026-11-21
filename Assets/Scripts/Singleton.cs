using UnityEngine;

// A GENERIC base class - the <T> means "this works for any class type."
// Any class that inherits from Singleton<ItsOwnName> automatically gets
// the "there's only ever one, and everyone can reach it" behaviour.
// Used by: GameManager, AudioManager.
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // THE one-and-only instance. Private, so nothing outside can overwrite it.
    private static T instance;

    // The public "front door" everyone uses. Example: GameManager.Instance.AddScore(1);
    public static T Instance
    {
        get
        {
            // If nobody has grabbed the instance yet, find it in the scene.
            // (FindAnyObjectByType replaced the deprecated FindFirstObjectByType.)
            if (instance == null)
            {
                instance = FindAnyObjectByType<T>();
            }
            return instance;
        }
    }

    // Runs automatically when this object is created in the scene.
    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;   // first one created becomes THE instance
        }
        else if (instance != this)
        {
            Destroy(gameObject);    // a second copy exists by mistake - remove it
        }
    }
}
