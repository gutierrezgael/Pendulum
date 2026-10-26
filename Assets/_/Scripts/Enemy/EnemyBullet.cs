using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyBullet : MonoBehaviour
{
    [Tooltip("Writes in the Console what the bullet touches (for testing)")]
    public bool debugHits = false;

    [Header("Sprite")]
    [Tooltip("Child object with the SpriteRenderer. It always faces the camera. Set its scale to 1 divided by the bullet size at which the art should look native (2.5 for size 0.4).")]
    public Transform sprite;

    [Header("End of flight")]
    [Tooltip("Seconds before the maximum range during which the bullet shrinks to nothing")]
    public float shrinkDuration = 0.2f;

    int damage = 1;
    float range;
    float travelled;
    Vector3 velocity;
    Vector3 baseScale = Vector3.one;
    Rigidbody rb;

    public void Launch(Vector3 direction, float speed, float size, float range, int damage)
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        this.damage = damage;
        this.range = range;
        velocity = direction.normalized * speed;

        baseScale = Vector3.one * size;
        transform.localScale = baseScale;
        rb.linearVelocity = velocity;
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // Chronomètre Cassé: the bullet stops in mid-air, and its lifetime stops too
        if (EnemyChaser.IsFrozen)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }
        rb.linearVelocity = velocity;

        float speed = velocity.magnitude;
        float step = speed * Time.fixedDeltaTime;
        travelled += step;
        if (travelled >= range)
        {
            Destroy(gameObject);
            return;
        }

        // Shrinks to nothing just before the maximum range (the collider shrinks with it)
        float shrinkDistance = speed * shrinkDuration;
        float k = shrinkDistance > 0f ? Mathf.Clamp01((range - travelled) / shrinkDistance) : 1f;
        transform.localScale = baseScale * k;

        // Walls: a ray along the flight direction (never touches the floor, whatever the size)
        float reach = transform.localScale.x * 0.5f + step;
        if (!Physics.Raycast(transform.position, velocity.normalized, out RaycastHit hit, reach,
                             ~0, QueryTriggerInteraction.Ignore)) return;

        Collider other = hit.collider;
        if (other.GetComponentInParent<EnemyHealth>() != null) return;       // passes through enemies
        if (other.GetComponentInParent<PersistentPlayer>() != null) return;  // the player is handled below
        if (other.GetComponentInParent<ThirdPersonController>() != null) return;
        if (other.GetComponentInParent<Junior>() != null) return;

        if (debugHits) Debug.Log($"Enemy bullet stopped by '{other.name}'");
        Destroy(gameObject);
    }

    void LateUpdate()
    {
        Camera cam = Camera.main;
        if (sprite != null && cam != null)
            sprite.rotation = cam.transform.rotation;
    }

    // The player
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PersistentPlayer>() == null) return;

        if (debugHits) Debug.Log("Enemy bullet hit the player");

        Health health = FindAnyObjectByType<Health>();
        PlayerInvincibility.Hurt(health, damage); // does nothing while Cloche is protected

        Destroy(gameObject);
    }
}