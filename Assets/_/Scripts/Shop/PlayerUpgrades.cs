using StarterAssets;
using UnityEngine;

/// <summary>
/// Put it on the player root (Player_Cloche, next to PersistentPlayer) and drag in every
/// permanent upgrade of the shop. When a new run starts, the levels bought in the shop
/// are added to Cloche's stats.
/// </summary>
public class PlayerUpgrades : MonoBehaviour
{
    [Tooltip("Every permanent upgrade of the shop")]
    public UpgradeData[] upgrades;

    // Called by PersistentPlayer once per run (not when a new floor takes over)
    public void Apply()
    {
        PlayerStats stats = GetComponentInChildren<PlayerStats>(true);
        ThirdPersonController movement = GetComponentInChildren<ThirdPersonController>(true);
        PlayerInventory inventory = GetComponent<PlayerInventory>();

        foreach (UpgradeData upgrade in upgrades)
        {
            if (upgrade == null) continue;

            float total = upgrade.Level * upgrade.amountPerLevel;
            if (total == 0f) continue;

            switch (upgrade.stat)
            {
                case UpgradeStat.Damage:
                    if (stats != null) stats.damage += Mathf.RoundToInt(total);
                    break;
                case UpgradeStat.FireRate:
                    if (stats != null) stats.fireRate += total;
                    break;
                case UpgradeStat.BulletSpeed:
                    if (stats != null) stats.bulletSpeed += total;
                    break;
                case UpgradeStat.BulletSize:
                    if (stats != null) stats.bulletSize += total;
                    break;
                case UpgradeStat.Range:
                    if (stats != null) stats.range += total;
                    break;
                case UpgradeStat.MoveSpeed:
                    if (movement != null)
                    {
                        movement.MoveSpeed += total;
                        movement.SprintSpeed += total;
                    }
                    break;
                case UpgradeStat.MaxHealth:
                    if (inventory != null) inventory.AddMaxHealthBonus(Mathf.RoundToInt(total));
                    break;
            }
        }
    }
}
