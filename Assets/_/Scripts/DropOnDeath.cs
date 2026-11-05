using UnityEngine;
public class DropOnDeath : MonoBehaviour
{
    [System.Serializable]
    public struct DropEntry
    {
        public GameObject prefab;

        public float weight;
    }

    public DropEntry[] drops =
    {
        new DropEntry { weight = 25f },
        new DropEntry { weight = 25f },
        new DropEntry { weight = 50f },
    };

    public Transform dropPoint;

    public float heightOffset = 0.7f;

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
