using UnityEngine;

public enum ItemType { Passive, Active }

// Internal only: used by the Item Pool to decide how often an item shows up. Never displayed in game.
public enum Rarity { Common, Uncommon, Rare, Legendary }

// New effects are added at the end so existing item assets keep their value
public enum ActiveEffect { Heal, Blast, Gear, AreaDamage, FreezeEnemies, Syringe }

[CreateAssetMenu(menuName = "Pendulum/Item", fileName = "NewItem")]
public class ItemData : ScriptableObject
{
    [Header("Info (in-game text: French)")]
    public string itemName = "Nouvel objet";
    [TextArea] public string description;
    public Sprite icon;
    public ItemType type = ItemType.Passive;
    [Tooltip("Internal only (never shown in game): how often an item room offers it. The odds are set in the Item Pool.")]
    public Rarity rarity = Rarity.Common;

    [Header("Passive: added to the player's stats (negative = worse)")]
    public int damageBonus;
    public float fireRateBonus;
    public float bulletSpeedBonus;
    public float bulletSizeBonus;
    public float rangeBonus;
    public float moveSpeedBonus;
    [Tooltip("Raises the maximum HP (also applies on the next floors)")]
    public int maxHealthBonus;
    [Tooltip("HP healed right when picked up")]
    public int healOnPickup;
    [Tooltip("Optional: a companion that appears on Cloche every time she enters a room (Junior). Needs the Junior script.")]
    public GameObject companionPrefab;

    [Header("Active: used with the key set on PlayerInventory")]
    public ActiveEffect activeEffect = ActiveEffect.Heal;
    [Tooltip("Seconds before it can be used again")]
    public float cooldown = 20f;
    [Tooltip("Heal: HP restored. Blast / Gear / AreaDamage: damage dealt. Syringe: damage bonus for the rest of the floor.")]
    public int effectAmount = 3;
    [Tooltip("Gear: projectile prefab (needs the Projectile script). AreaDamage: optional visual (the asbestos circle).")]
    public GameObject effectPrefab;

    [Header("AreaDamage")]
    [Tooltip("Radius around the player (a zone of about 6x6 = radius 3)")]
    public float radius = 3f;

    [Header("FreezeEnemies")]
    [Tooltip("Seconds")]
    public float duration = 4f;

    [Header("Syringe")]
    [Tooltip("HP lost when used (can't be used if it would kill the player)")]
    public int selfDamage = 20;

    [Header("Gear (projectile)")]
    public float projectileSpeed = 10f;
    public float projectileSize = 2f;
    public float projectileRange = 25f;
}