using UnityEngine;

/// <summary>
/// Picks one random reward (or nothing). Same weighted idea as the Item Pool, but for any object.
/// - "Prefab" entries are created where the enemy died / at the room's center.
/// - Entries that are objects already in the scene (placed and switched OFF, e.g. a coin in the
///   middle of the room) are simply switched on.
/// Triggers:
/// - On an enemy (next to EnemyHealth): when the enemy dies.
/// - On a room: when a CES calls DropOnDeath.DropNow (Invoke Events), or automatically when the
///   room is cleared if "Drop When Room Cleared" is ticked.
/// </summary>
public class DropOnDeath : MonoBehaviour
{
    [System.Serializable]
    public struct DropEntry
    {
        [Tooltip("A prefab (created when dropped) or an object already in the room that is switched OFF (it gets switched on). Leave empty for 'nothing'.")]
        public GameObject prefab;
        [Tooltip("Higher = more likely. The chance is this weight divided by the total of all weights.")]
        public float weight;
    }

    [Tooltip("Weights 25 / 25 / 50 = 25% first, 25% second, 50% third (leave the third empty for 'nothing')")]
    public DropEntry[] drops =
    {
        new DropEntry { weight = 25f },
        new DropEntry { weight = 25f },
        new DropEntry { weight = 50f },
    };

    [Tooltip("Optional: where prefabs appear. Empty = this object's position (the middle of the room, for a room).")]
    public Transform dropPoint;

    [Tooltip("Height above that point where prefabs appear")]
    public float heightOffset = 0.7f;

    [Tooltip("On a room: drop automatically as soon as the room is cleared (no CES needed). Don't use it on rooms that clear instantly (start room, item room).")]
    public bool dropWhenRoomCleared = false;

    void Awake()
    {
        EnemyHealth enemy = GetComponent<EnemyHealth>();
        if (enemy != null) enemy.onDeath.AddListener(DropNow);

        RoomController room = GetComponent<RoomController>();
        if (room != null && dropWhenRoomCleared) room.onRoomCleared.AddListener(DropNow);
    }

    // Public so a CES (Invoke Events) can call it
    public void DropNow()
    {
        float total = 0f;
        foreach (DropEntry entry in drops) total += Mathf.Max(0f, entry.weight);
        if (total <= 0f) return;

        float roll = Random.value * total;
        foreach (DropEntry entry in drops)
        {
            float weight = Mathf.Max(0f, entry.weight);
            if (weight <= 0f) continue;

            if (roll <= weight)
            {
                Give(entry.prefab);
                return;
            }
            roll -= weight;
        }
    }

    void Give(GameObject reward)
    {
        if (reward == null) return; // "nothing"

        // Already in the scene (switched off): just switch it on
        if (reward.scene.IsValid())
        {
            reward.SetActive(true);
            return;
        }

        // A prefab: create it
        Vector3 center = (dropPoint != null ? dropPoint : transform).position;
        Instantiate(reward, center + Vector3.up * heightOffset, reward.transform.rotation);
    }
}
