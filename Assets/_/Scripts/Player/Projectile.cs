using StarterAssets;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Tooltip("Writes in the Console what the bullet touches (for testing)")]
    public bool debugHits = false;

    [Tooltip("Goes through enemies instead of disappearing (the Engrenage Perdu)")]
    public bool pierceEnemies = false;

    [Header("Sprite")]
    [Tooltip("Child object with the SpriteRenderer. It always faces the camera. Set its scale to 1 divided by the bullet size at which the art should look native (3.33 for size 0.3).")]
    public Transform sprite;

    [Header("End of flight")]
    [Tooltip("Seconds before the maximum range during which the bullet shrinks to nothing")]
    public float shrinkDuration = 0.2f;

    int damage = 1;
    Vector3 flightDirection;
    float flightSpeed;
    float lifetime;
    float age;
    Vector3 baseScale = Vector3.one;

    public void Launch(Vector3 direction, float speed, float size, float range, int damage)
    {
        this.damage = damage;
        flightDirection = direction.normalized;
        flightSpeed = speed;
        lifetime = range / speed; // lifetime = distance / speed
        age = 0f;

        baseScale = Vector3.one * size;
        transform.localScale = baseScale;

        GetComponent<Rigidbody>().linearVelocity = direction * speed;
    }

    void Update()
    {
        if (flightSpeed <= 0f) return;

        age += Time.deltaTime;
        float remaining = lifetime - age;
        if (remaining <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        // Shrinks to nothing just before the maximum range (the collider shrinks with it)
        float k = shrinkDuration > 0f ? Mathf.Clamp01(remaining / shrinkDuration) : 1f;
        transform.localScale = baseScale * k;
    }

    void LateUpdate()
    {
        Camera cam = Camera.main;
        if (sprite != null && cam != null)
            sprite.rotation = cam.transform.rotation;
    }

    // Walls: a ray along the flight direction. Unlike the bullet's own collider, it never
    // touches the floor, so big bullets don't vanish the moment they are fired.
    void FixedUpdate()
    {
        if (flightSpeed <= 0f) return;

        float reach = transform.localScale.x * 0.5f + flightSpeed * Time.fixedDeltaTime;
        if (!Physics.Raycast(transform.position, flightDirection, out RaycastHit hit, reach,
                             ~0, QueryTriggerInteraction.Ignore)) return;

        Collider other = hit.collider;
        if (other.GetComponentInParent<EnemyHealth>() != null) return;       // enemies are handled below
        if (other.GetComponentInParent<ThirdPersonController>() != null) return;
        if (other.GetComponentInParent<PersistentPlayer>() != null) return;
        if (other.GetComponentInParent<Junior>() != null) return;

        if (debugHits) Debug.Log($"Bullet stopped by '{other.name}'");
        Destroy(gameObject);
    }

    // Enemies: damage on contact
    void OnTriggerEnter(Collider other)
    {
        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy == null) return;

        if (debugHits) Debug.Log($"Bullet hit enemy '{enemy.name}'");

        enemy.TakeDamage(damage);
        if (!pierceEnemies) Destroy(gameObject);
    }
}