using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pendulum/Item Pool", fileName = "ItemPool")]
public class ItemPool : ScriptableObject
{
    public List<ItemData> items = new List<ItemData>();

    /// <summary>Random item the player doesn't own yet (null if there's none left).</summary>
    public ItemData PickRandom(PlayerInventory owned)
    {
        List<ItemData> candidates = new List<ItemData>();
        foreach (ItemData item in items)
        {
            if (item == null) continue;
            if (owned != null && owned.Has(item)) continue;
            candidates.Add(item);
        }

        if (candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }
}