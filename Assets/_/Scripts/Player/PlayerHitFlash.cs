using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerHitFlash : MonoBehaviour
{
    public Color flashColor = Color.red;
    public float flashTime = 0.15f;

    SpriteRenderer sr;
    Color baseColor;
    Health health;
    int lastValue;
    float flashTimer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
    }

    void Update()
    {
        // Look for the Health of the current scene (it changes when a new floor loads)
        if (health == null)
        {
            health = FindAnyObjectByType<Health>();
            if (health == null) return;
            lastValue = health.Value;
        }

        // Health went down: flash
        if (health.Value < lastValue)
        {
            sr.color = flashColor;
            flashTimer = flashTime;
        }
        lastValue = health.Value;

        // Back to normal color after the flash
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f) sr.color = baseColor;
        }
    }
}