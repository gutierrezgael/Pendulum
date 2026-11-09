using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pendulum/Item Pool", fileName = "ItemPool")]
public class ItemPool : ScriptableObject
{
    public List<ItemData> items = new List<ItemData>();

    [Header("Odds per rarity (higher = more often, 0 = never)")]
    public float commonWeight = 60f;
    public float uncommonWeight = 25f;
    public float rareWeight = 12f;
    public float legendaryWeight = 3f;

    const int RarityCount = 4;

    float WeightOf(int rarityIndex)
    {
        switch ((Rarity)rarityIndex)
        {
            case Rarity.Uncommon: return uncommonWeight;
            case Rarity.Rare: return rareWeight;
            case Rarity.Legendary: return legendaryWeight;
            default: return commonWeight;
        }
    }

    /// <summary>Random item the player doesn't own yet, respecting the rarity odds (null if none left).</summary>
    public ItemData PickRandom(PlayerInventory owned, ICollection<ItemData> exclude = null)
    {
        // Items still available, grouped by rarity
        List<ItemData>[] byRarity = new List<ItemData>[RarityCount];
        for (int i = 0; i < RarityCount; i++) byRarity[i] = new List<ItemData>();

        List<ItemData> all = new List<ItemData>();
        foreach (ItemData item in items)
        {
            if (item == null) continue;
            if (owned != null && owned.Has(item)) continue;
            if (exclude != null && exclude.Contains(item)) continue; // already on another pedestal
            byRarity[(int)item.rarity].Add(item);
            all.Add(item);
        }

        if (all.Count == 0) return null;

        // Only tiers that still have items take part in the roll
        float total = 0f;
        for (int i = 0; i < RarityCount; i++)
            if (byRarity[i].Count > 0) total += Mathf.Max(0f, WeightOf(i));

        // All remaining tiers have weight 0: pick any item
        if (total <= 0f) return all[Random.Range(0, all.Count)];

        float roll = Random.value * total;
        for (int i = 0; i < RarityCount; i++)
        {
            if (byRarity[i].Count == 0) continue;

            float weight = Mathf.Max(0f, WeightOf(i));
            if (weight <= 0f) continue;

            if (roll <= weight)
                return byRarity[i][Random.Range(0, byRarity[i].Count)];

            roll -= weight;
        }

        return all[Random.Range(0, all.Count)];
    }
}