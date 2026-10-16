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
        PlayerSpawn spawn = FindAnyObjectByType<PlayerSpawn>();
        if (spawn == null) return;

        // The CharacterController overrides teleports unless it's disabled for a moment
        CharacterController cc = GetComponentInChildren<CharacterController>();
        if (cc != null) cc.enabled = false;

        // Move the whole player so the CharacterController's object (the part that actually
        // moves) lands exactly on the spawn point. Moving only the root would also apply
        // any height/offset between the root and the capsule, so the player jumps on Play.
        transform.rotation = spawn.transform.rotation;
        Transform body = cc != null ? cc.transform : transform;
        transform.position += spawn.transform.position - body.position;

        if (cc != null) cc.enabled = true;
    }
}