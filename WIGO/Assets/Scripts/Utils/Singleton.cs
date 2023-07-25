using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    GameObject singleton = new GameObject();
                    _instance = singleton.AddComponent<T>();
                    singleton.name = typeof(T).ToString() + " (singleton)";
                }

                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    private void Awake()
    {
        if (_instance != null)
        {
            if (_instance != this)
                Destroy(gameObject);
        }
        else
            _instance = GetComponent<T>();

        Init();
    }

    protected virtual void Init() { }
}
