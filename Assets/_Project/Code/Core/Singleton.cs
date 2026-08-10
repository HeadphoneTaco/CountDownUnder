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

    // Override and return true to survive scene loads. Off by default, because most
    // managers are tied to one scene and a stray survivor causes stranger bugs than a
    // missing one. AudioManager turns this on so music carries from menu into gameplay.
    protected virtual bool PersistAcrossScenes => false;

    // First instance claims the slot; any later duplicate destroys itself.
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;

            if (PersistAcrossScenes)
            {
                // DontDestroyOnLoad only works on root objects. On a child it logs a
                // warning and silently does nothing, so detach first and say so.
                if (transform.parent != null)
                {
                    Debug.Log($"[Singleton] Detaching {typeof(T).Name} from '{transform.parent.name}' " +
                              "so it can survive scene loads.", this);
                    transform.SetParent(null);
                }

                DontDestroyOnLoad(gameObject);
            }
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
}
