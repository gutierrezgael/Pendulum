using UnityEngine;

public class PlayerInvincibility : MonoBehaviour
{
    public static PlayerInvincibility Instance { get; private set; }

    [Tooltip("Seconds Cloche can't be hurt after taking damage (shared by every enemy and bullet)")]
    public float duration = 1f;

    [Header("Feedback")]
    [Tooltip("Cloche's sprite blinks while she is protected")]
    public bool blink = true;
    public float blinkInterval = 0.08f;

    SpriteRenderer sprite;
    float invincibleUntil;
    bool registered;

    public bool IsInvincible => Time.time < invincibleUntil;

    void Awake()
    {
        // A second copy of the player (new floor) is destroyed by PersistentPlayer: ignore it
        if (Instance != null && Instance != this)
        {
            enabled = false;
            return;
        }

        Instance = this;
        registered = true;

        PlayerSpriteAnimator animator = GetComponentInChildren<PlayerSpriteAnimator>(true);
        if (animator != null) sprite = animator.GetComponent<SpriteRenderer>();
    }

    void OnDestroy()
    {
        if (registered && Instance == this) Instance = null;
    }

    void Update()
    {
        if (!blink || sprite == null) return;

        if (IsInvincible)
            sprite.enabled = Mathf.FloorToInt(Time.time / Mathf.Max(0.01f, blinkInterval)) % 2 == 0;
        else if (!sprite.enabled)
            sprite.enabled = true;
    }

    /// <summary>
    /// Every enemy damage goes through here. Returns true if the damage was applied,
    /// false if Cloche is still protected from a previous hit.
    /// </summary>
    public static bool Hurt(Health health, int amount)
    {
        if (health == null) return false;

        PlayerInvincibility protection = Instance;
        if (protection != null)
        {
            if (protection.IsInvincible) return false;
            protection.invincibleUntil = Time.time + protection.duration;
        }

        health.Remove(amount);
        return true;
    }
}