using PixelComrades;
using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour {
    private static T _main;
    private static object _lock = new object();
    protected static bool IsSingletonNull { get { return _main == null; } }
    public static T main {
        get {
            if (_main != null) {
                return _main;
            }
            if (TimeManager.IsQuitting) {
                _main = (T)FindObjectOfType(typeof(T));
                return _main;
            }
            lock (_lock) {
                if (_main == null) {
                    _main = (T)FindObjectOfType(typeof(T));
                    if (FindObjectsOfType(typeof(T)).Length > 1) {
                        Debug.LogError("[Singleton] Something went really wrong " +
                            " - there should never be more than 1 singleton!" +
                            " Reopenning the scene might fix it.");
                        return _main;
                    }
                    if (_main == null) {
                        GameObject singleton = new GameObject();
                        _main = singleton.AddComponent<T>();
                        singleton.name = "(singleton) " + typeof(T).ToString();
                        //DontDestroyOnLoad(singleton);
                        Debug.Log("[Singleton] An main of " + typeof(T) +
                            " is needed in the scene, so '" + singleton +
                            "' was created with DontDestroyOnLoad.");
                    }
                }
                return _main;
            }
        }
        protected set { _main = value; }
    }
}