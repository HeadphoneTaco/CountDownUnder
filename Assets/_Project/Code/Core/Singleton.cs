using UnityEngine;

// Base class giving any MonoBehaviour a single shared instance via Instance.
// Usage: public class LevelSingleton : Singleton<LevelSingleton> { ... }
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                // Find one in the scene, or create one if none exists.
                _instance = FindFirstObjectByType<T>();
                if (_instance == null)
                {
                    GameObject go = new GameObject(typeof(T).Name + " (Singleton)");
                    _instance = go.AddComponent<T>();
                    Debug.Log("[Singleton] Auto-created " + typeof(T).Name + " because no instance was in the scene.");
                }
            }

            return _instance;
        }
    }

    // First instance claims the slot; any later duplicate destroys itself.
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
}
