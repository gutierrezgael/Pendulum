using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentPlayer : MonoBehaviour
{
    public static PersistentPlayer Instance { get; private set; }

    void Awake()
    {
        // If a player already survived from the previous floor, this new copy is a duplicate
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayerSpawn spawn = FindFirstObjectByType<PlayerSpawn>();
        if (spawn == null) return;

        // The CharacterController overrides teleports unless it's disabled for a moment
        CharacterController cc = GetComponentInChildren<CharacterController>();
        if (cc != null) cc.enabled = false;

        transform.SetPositionAndRotation(spawn.transform.position, spawn.transform.rotation);

        if (cc != null) cc.enabled = true;
    }
}