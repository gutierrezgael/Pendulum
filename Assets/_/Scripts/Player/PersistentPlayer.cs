using StarterAssets;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-10000)] // runs before the other scripts of the new floor, so the old player is gone first
public class PersistentPlayer : MonoBehaviour
{
    public static PersistentPlayer Instance { get; private set; }

    bool tookOver;

    void Awake()
    {
        // A player from the previous floor exists: this scene's copy takes over from it
        if (Instance != null && Instance != this)
        {
            PersistentPlayer previous = Instance;
            tookOver = true;
            TransferFrom(previous);

            // Removed on the spot (its OnDestroy clears Instance), so there are never two players
            DestroyImmediate(previous.gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // A brand new run (not a new floor taking over): the permanent upgrades bought in the shop apply
        if (!tookOver)
        {
            PlayerUpgrades permanent = GetComponent<PlayerUpgrades>();
            if (permanent != null) permanent.Apply();
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Once everything has started: pair the new player's input with the keyboard and mouse again.
    // (The previous player held them until it was removed, so the new one could end up with no input.)
    void Start()
    {
        if (!tookOver) return;

        foreach (PlayerInput input in GetComponentsInChildren<PlayerInput>(true))
        {
            input.enabled = false;
            input.enabled = true;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    // What the previous floor's Cloche has earned: stats changed by items, items, cooldowns
    void TransferFrom(PersistentPlayer previous)
    {
        PlayerStats oldStats = previous.GetComponentInChildren<PlayerStats>(true);
        PlayerStats newStats = GetComponentInChildren<PlayerStats>(true);
        if (oldStats != null && newStats != null)
        {
            newStats.damage = oldStats.damage;
            newStats.fireRate = oldStats.fireRate;
            newStats.bulletSpeed = oldStats.bulletSpeed;
            newStats.bulletSize = oldStats.bulletSize;
            newStats.range = oldStats.range;
        }

        ThirdPersonController oldMove = previous.GetComponentInChildren<ThirdPersonController>(true);
        ThirdPersonController newMove = GetComponentInChildren<ThirdPersonController>(true);
        if (oldMove != null && newMove != null)
        {
            newMove.MoveSpeed = oldMove.MoveSpeed;
            newMove.SprintSpeed = oldMove.SprintSpeed;
        }

        PlayerInventory oldInventory = previous.GetComponent<PlayerInventory>();
        PlayerInventory newInventory = GetComponent<PlayerInventory>();
        if (oldInventory != null && newInventory != null)
            newInventory.CopyFrom(oldInventory);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayerSpawn spawn = FindAnyObjectByType<PlayerSpawn>();
        if (spawn == null) return;

        // The CharacterController overrides teleports unless it's disabled for a moment
        CharacterController cc = GetComponentInChildren<CharacterController>();
        if (cc != null) cc.enabled = false;

        // The CharacterController's object (the part that actually moves) lands exactly on the spawn
        transform.rotation = spawn.transform.rotation;
        Transform body = cc != null ? cc.transform : transform;
        transform.position += spawn.transform.position - body.position;

        if (cc != null) cc.enabled = true;
    }
}