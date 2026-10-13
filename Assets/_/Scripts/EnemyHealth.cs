using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;

    [Header("Hit feedback (optional)")]
    public SpriteRenderer spriteToFlash;
    public Color flashColor = Color.red;
    public float flashTime = 0.08f;

    [Header("Events")]
    public UnityEvent onDamaged;
    public UnityEvent onDeath;

    // The room listens to this to count remaining enemies
    public System.Action<EnemyHealth> Died;

    int current;
    Color baseColor = Color.white;
    float flashTimer;

    public bool IsDead => current <= 0;

    void Awake()
    {
        current = maxHealth;
        if (spriteToFlash != null) baseColor = spriteToFlash.color;
    }

    void Update()
    {
        if (spriteToFlash != null && flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f) spriteToFlash.color = baseColor;
        }
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        current -= amount;
        onDamaged.Invoke();

        if (spriteToFlash != null)
        {
            spriteToFlash.color = flashColor;
            flashTimer = flashTime;
        }

        if (current <= 0)
        {
            current = 0;
            onDeath.Invoke();
            Died?.Invoke(this);
            Destroy(gameObject);
        }
    }
}