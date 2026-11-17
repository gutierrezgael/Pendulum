using UnityEngine;
using ColliderEventSystem;

public enum UpgradeStat { Damage, FireRate, BulletSpeed, BulletSize, Range, MoveSpeed, MaxHealth }

/// <summary>
/// One permanent upgrade of the shop = one asset. Create with: Assets > Create > Pendulum > Permanent Upgrade.
/// The shop sells it (ShopUpgrade) and every new run starts with it applied (PlayerUpgrades).
/// </summary>
[CreateAssetMenu(menuName = "Pendulum/Permanent Upgrade", fileName = "NewUpgrade")]
public class UpgradeData : ScriptableObject
{
    [Tooltip("A CES IntVariable with Persistent ticked, Initial Value 0. It counts how many times this upgrade was bought.")]
    public IntVariable level;

    [Tooltip("What it improves")]
    public UpgradeStat stat = UpgradeStat.Damage;

    [Tooltip("Added to the stat for each level bought")]
    public float amountPerLevel = 1f;

    [Tooltip("Price of each level: the first number is level 1, and so on. The number of prices is the maximum level.")]
    public int[] prices = { 5, 10, 15, 20, 25 };

    public int MaxLevel => prices.Length;
    public int Level => level != null ? Mathf.Clamp(level.RuntimeValue, 0, MaxLevel) : 0;
    public bool IsMaxed => Level >= MaxLevel;
    public int NextPrice => IsMaxed ? 0 : prices[Level];
}
