using System.Collections;
using UnityEngine;
using UnityEngine.Events;


public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;

    [Header("Hit feedback (optional)")]
    public SpriteRenderer spriteToFlash;
    public Color flashColor = Color.red;
    public float flashTime = 0.08f;

    [Header("Death: red fade-out")]
    public Color deathColor = Color.red;
    [Tooltip("Seconds the enemy takes to fade out. 0 = disappears at once.")]
    public float deathFadeTime = 0.4f;

    [Header("Events")]
    public UnityEvent onDamaged;
    public UnityEvent onDeath;

    // The room listens to this to count remaining enemies
    public System.Action<EnemyHealth> Died;

    int current;
    Color baseColor = Color.white;
    float flashTimer;

    // True from the moment of the lethal hit (the fade-out is still playing)
    public bool IsDead => current <= 0;

    void Awake()
    {
        current = maxHealth;
        if (spriteToFlash != null) baseColor = spriteToFlash.color;
    }

    void Update()
    {
        if (IsDead) return; // the death fade controls the color now

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

        if (current <= 0)
        {
            current = 0;
            Die();
            return;
        }

        if (spriteToFlash != null)
        {
            spriteToFlash.color = flashColor;
            flashTimer = flashTime;
        }
    }

    void Die()
    {
        onDeath.Invoke();
        Died?.Invoke(this); // the room counts it as dead right away

        // Nothing can hit it anymore (bullets, Junior...)
        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        StartCoroutine(DeathFade());
    }

    IEnumerator DeathFade()
    {
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        Light[] lights = GetComponentsInChildren<Light>();

        float[] lightStart = new float[lights.Length];
        for (int i = 0; i < lights.Length; i++) lightStart[i] = lights[i].intensity;

        float t = 0f;
        while (t < deathFadeTime)
        {
            float k = Mathf.Clamp01(t / deathFadeTime); // 0 = just died, 1 = gone

            Color c = deathColor;
            c.a = 1f - k;
            foreach (SpriteRenderer s in sprites)
                if (s != null) s.color = c;

            for (int i = 0; i < lights.Length; i++)
                if (lights[i] != null) lights[i].intensity = lightStart[i] * (1f - k);

            t += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}