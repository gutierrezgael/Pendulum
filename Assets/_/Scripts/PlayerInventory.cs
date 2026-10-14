using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInventory : MonoBehaviour
{
    [Header("Controls")]
    public Key useActiveKey = Key.E;

    [Header("Owned items (read only)")]
    public List<ItemData> passives = new List<ItemData>();
    public ItemData activeItem;

    PlayerStats stats;
    ThirdPersonController controller;
    float activeReadyTime;
    int bonusMaxHealth;
    int floorDamageBonus; // Syringe: lasts until the next floor
    readonly List<GameObject> spawnedCompanions = new List<GameObject>();

    public float ActiveCooldownLeft => Mathf.Max(0f, activeReadyTime - Time.time);

    public bool Has(ItemData item) => passives.Contains(item) || activeItem == item;

    void Awake()
    {
        stats = GetComponentInChildren<PlayerStats>(true);
        controller = GetComponentInChildren<ThirdPersonController>(true);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // New floor: the Syringe's damage bonus ends
        if (floorDamageBonus != 0 && stats != null)
        {
            stats.damage = Mathf.Max(1, stats.damage - floorDamageBonus);
            floorDamageBonus = 0;
        }

        // Each floor has its own Health: give it the max HP earned so far
        if (bonusMaxHealth != 0)
        {
            Health health = FindAnyObjectByType<Health>();
            if (health != null) health.maximum += bonusMaxHealth;
        }
    }

    void Update()
    {
        if (activeItem == null || Keyboard.current == null) return;

        if (Keyboard.current[useActiveKey].wasPressedThisFrame && Time.time >= activeReadyTime)
            UseActive();
    }

    public void Acquire(ItemData item)
    {
        if (item == null) return;

        if (item.type == ItemType.Passive)
        {
            passives.Add(item);
            ApplyPassive(item);
        }
        else
        {
            // A new active item replaces the old one
            activeItem = item;
            activeReadyTime = Time.time; // ready right away
        }
    }

    // --- Passive items ---

    void ApplyPassive(ItemData item)
    {
        if (stats != null)
        {
            stats.damage = Mathf.Max(1, stats.damage + item.damageBonus);
            stats.fireRate = Mathf.Max(0.5f, stats.fireRate + item.fireRateBonus);
            stats.bulletSpeed = Mathf.Max(3f, stats.bulletSpeed + item.bulletSpeedBonus);
            stats.bulletSize = Mathf.Max(0.1f, stats.bulletSize + item.bulletSizeBonus);
            stats.range = Mathf.Max(3f, stats.range + item.rangeBonus);
        }

        if (controller != null && item.moveSpeedBonus != 0f)
        {
            controller.MoveSpeed = Mathf.Max(1f, controller.MoveSpeed + item.moveSpeedBonus);
            controller.SprintSpeed = Mathf.Max(1f, controller.SprintSpeed + item.moveSpeedBonus);
        }

        // Picked up a companion: it shows up on Cloche right away
        if (item.companionPrefab != null) RespawnCompanions();

        Health health = FindAnyObjectByType<Health>();
        if (health == null) return;

        if (item.maxHealthBonus != 0)
        {
            health.maximum += item.maxHealthBonus;
            bonusMaxHealth += item.maxHealthBonus;
            if (item.maxHealthBonus > 0) health.Add(item.maxHealthBonus);
        }

        if (item.healOnPickup > 0) health.Add(item.healOnPickup);
    }

    // --- Companions (Junior) ---

    // Called by every RoomController when the player enters a room:
    // old companions are removed, new ones appear on Cloche.
    public void RespawnCompanions()
    {
        foreach (GameObject companion in spawnedCompanions)
            if (companion != null) Destroy(companion);
        spawnedCompanions.Clear();

        Vector3 spawnPosition = (controller != null ? controller.transform : transform).position;

        foreach (ItemData item in passives)
        {
            if (item.companionPrefab == null) continue;
            spawnedCompanions.Add(Instantiate(item.companionPrefab, spawnPosition, Quaternion.identity));
        }
    }

    // --- Active item ---

    void UseActive()
    {
        bool used = false;

        switch (activeItem.activeEffect)
        {
            case ActiveEffect.Heal: used = DoHeal(); break;
            case ActiveEffect.Blast: used = DoBlast(); break;
            case ActiveEffect.Gear: used = DoGear(); break;
            case ActiveEffect.AreaDamage: used = DoAreaDamage(); break;
            case ActiveEffect.FreezeEnemies: used = DoFreeze(); break;
            case ActiveEffect.Syringe: used = DoSyringe(); break;
        }

        // If it couldn't be used, the cooldown doesn't start
        if (used) activeReadyTime = Time.time + activeItem.cooldown;
    }

    Transform PlayerBody => controller != null ? controller.transform : transform;

    bool DoHeal()
    {
        Health health = FindAnyObjectByType<Health>();
        if (health == null) return false;
        health.Add(activeItem.effectAmount);
        return true;
    }

    // Damages every enemy that is awake
    bool DoBlast()
    {
        foreach (EnemyHealth enemy in FindObjectsByType<EnemyHealth>(FindObjectsInactive.Exclude))
            enemy.TakeDamage(activeItem.effectAmount);
        return true;
    }

    // Engrenage Perdu: a huge piercing projectile toward the mouse
    bool DoGear()
    {
        PlayerShooter shooter = GetComponentInChildren<PlayerShooter>(true);
        Camera cam = Camera.main;
        if (shooter == null || shooter.shootPoint == null || cam == null || Mouse.current == null) return false;
        if (activeItem.effectPrefab == null) return false;

        Projectile prefab = activeItem.effectPrefab.GetComponent<Projectile>();
        if (prefab == null) return false;

        Plane aimPlane = new Plane(Vector3.up, shooter.shootPoint.position);
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!aimPlane.Raycast(ray, out float distance)) return false;

        Vector3 direction = ray.GetPoint(distance) - shooter.shootPoint.position;
        direction.y = 0f;
        direction.Normalize();

        Projectile gear = Instantiate(prefab, shooter.shootPoint.position, Quaternion.LookRotation(direction));
        gear.pierceEnemies = true;
        gear.Launch(direction, activeItem.projectileSpeed, activeItem.projectileSize,
                    activeItem.projectileRange, activeItem.effectAmount);
        return true;
    }

    // Bombe d'Asbestos: damages every enemy around the player
    bool DoAreaDamage()
    {
        Vector3 center = PlayerBody.position;

        HashSet<EnemyHealth> alreadyHit = new HashSet<EnemyHealth>();
        foreach (Collider c in Physics.OverlapSphere(center, activeItem.radius))
        {
            EnemyHealth enemy = c.GetComponentInParent<EnemyHealth>();
            if (enemy != null && alreadyHit.Add(enemy))
                enemy.TakeDamage(activeItem.effectAmount);
        }

        // Optional visual (the asbestos circle)
        if (activeItem.effectPrefab != null)
        {
            GameObject fx = Instantiate(activeItem.effectPrefab, center + Vector3.up * 0.05f, Quaternion.identity);
            Destroy(fx, 1.5f);
        }
        return true;
    }

    // Chronomètre Cassé: every enemy stops for a few seconds
    bool DoFreeze()
    {
        EnemyChaser.FreezeAll(activeItem.duration);
        return true;
    }

    // Seringue de Mercure: lose HP now, more damage until the end of the floor
    bool DoSyringe()
    {
        Health health = FindAnyObjectByType<Health>();
        if (health == null || stats == null) return false;
        if (health.Value <= activeItem.selfDamage) return false; // can't kill yourself with it

        health.Remove(activeItem.selfDamage);
        stats.damage += activeItem.effectAmount;
        floorDamageBonus += activeItem.effectAmount;
        return true;
    }
}