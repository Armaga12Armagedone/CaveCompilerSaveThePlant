using UnityEngine;

public class DatabaseInitializer : MonoBehaviour
{
    public GameDatabase database;

    void Awake()
    {
        if (GameDatabase.Instance == null)
        {
            GameDatabase.Instance = database;
            GameDatabase.Instance.Initialize();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}