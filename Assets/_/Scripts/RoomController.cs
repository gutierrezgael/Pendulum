using UnityEngine;
using UnityEngine.Events;
using ColliderEventSystem;

public class RoomController : MonoBehaviour
{
    [Header("State (read only)")]
    [SerializeField] int enemiesAlive;
    [SerializeField] bool isCleared;

    [Header("CES link (optional)")]
    [Tooltip("CES IntVariable kept at the number of enemies left. 0 = room cleared, usable in a CES condition (e.g. to open doors).")]
    public IntVariable enemiesLeftVariable;

    [Header("Events")]
    [Tooltip("Open doors, spawn a reward, play a sound...")]
    public UnityEvent onRoomCleared;

    public bool IsCleared => isCleared;
    public int EnemiesAlive => enemiesAlive;

    void Start()
    {
        EnemyHealth[] enemies = GetComponentsInChildren<EnemyHealth>(true);
        enemiesAlive = enemies.Length;

        foreach (EnemyHealth e in enemies)
            e.Died += OnEnemyDied;

        UpdateVariable();

        // A room with no enemies (item room, start room) is cleared right away
        if (enemiesAlive == 0) Clear();
    }

    void OnEnemyDied(EnemyHealth enemy)
    {
        enemy.Died -= OnEnemyDied;
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        UpdateVariable();

        if (enemiesAlive == 0) Clear();
    }

    void Clear()
    {
        if (isCleared) return;
        isCleared = true;
        onRoomCleared.Invoke();
    }

    void UpdateVariable()
    {
        if (enemiesLeftVariable != null)
            enemiesLeftVariable.RuntimeValue = enemiesAlive;
    }
}