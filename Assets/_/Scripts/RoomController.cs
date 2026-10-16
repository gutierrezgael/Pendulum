using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ColliderEventSystem;
public class RoomController : MonoBehaviour
{
    [System.Serializable]
    public class SpawnEntry
    {
        [Tooltip("Empty object marking where the enemy appears")]
        public Transform point;
        [Tooltip("Enemy prefab (needs EnemyHealth)")]
        public EnemyHealth enemyPrefab;
    }

    [Header("Start")]
    [Tooltip("Checked: nothing happens until StartRoom() is called (by your CES). Unchecked: starts right away (start room, item room).")]
    public bool waitForPlayer = true;

    [Header("Enemies spawned when the room starts")]
    public SpawnEntry[] spawns;

    [Header("State (read only)")]
    [SerializeField] int enemiesAlive;
    [SerializeField] bool hasStarted;
    [SerializeField] bool isCleared;

    [Header("CES link (optional)")]
    [Tooltip("Kept at the number of enemies left. 0 = nobody left.")]
    public IntVariable enemiesLeftVariable;
    [Tooltip("False until the room is beaten, then true. Use it in your door's CES Variable condition.")]
    public BoolVariable clearedVariable;

    [Header("Events")]
    [Tooltip("Close the doors, play a sound...")]
    public UnityEvent onRoomStarted;
    [Tooltip("Open the doors, spawn a reward...")]
    public UnityEvent onRoomCleared;
    [Tooltip("Every time the player comes in (not only the first time): items appear, etc.")]
    public UnityEvent onPlayerEntered;

    // Enemies placed by hand in the room: they sleep until the room starts
    readonly List<EnemyHealth> placedEnemies = new List<EnemyHealth>();

    public bool IsCleared => isCleared;
    public bool HasStarted => hasStarted;
    public int EnemiesAlive => enemiesAlive;

    void Start()
    {
        foreach (EnemyHealth e in GetComponentsInChildren<EnemyHealth>(true))
        {
            placedEnemies.Add(e);
            Track(e);
            if (waitForPlayer) e.gameObject.SetActive(false);
        }

        UpdateVariables();

        if (!waitForPlayer) BeginRoom();
    }

    // --- Call this from the CES (Invoke Events) when the player enters ---
    public void StartRoom()
    {
        // Every time the player comes in, companions (Junior) respawn on Cloche
        PlayerInventory inventory = FindAnyObjectByType<PlayerInventory>();
        if (inventory != null) inventory.RespawnCompanions();

        onPlayerEntered.Invoke();

        BeginRoom();
    }

    // The fight / spawning itself only happens once
    void BeginRoom()
    {
        if (hasStarted || isCleared) return;
        hasStarted = true;

        foreach (EnemyHealth e in placedEnemies)
            if (e != null) e.gameObject.SetActive(true);

        foreach (SpawnEntry s in spawns)
        {
            if (s.point == null || s.enemyPrefab == null) continue;
            EnemyHealth e = Instantiate(s.enemyPrefab, s.point.position, s.point.rotation, transform);
            Track(e);
        }

        UpdateVariables();
        onRoomStarted.Invoke();

        // Nothing to fight (item room, empty room): cleared right away
        if (enemiesAlive == 0) Clear();
    }

    void Track(EnemyHealth enemy)
    {
        enemiesAlive++;
        enemy.Died += OnEnemyDied;
    }

    void OnEnemyDied(EnemyHealth enemy)
    {
        enemy.Died -= OnEnemyDied;
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        UpdateVariables();

        if (enemiesAlive == 0 && hasStarted) Clear();
    }

    void Clear()
    {
        if (isCleared) return;
        isCleared = true;
        UpdateVariables();
        onRoomCleared.Invoke();
    }

    void UpdateVariables()
    {
        if (enemiesLeftVariable != null) enemiesLeftVariable.RuntimeValue = enemiesAlive;
        if (clearedVariable != null) clearedVariable.RuntimeValue = isCleared;
    }
}