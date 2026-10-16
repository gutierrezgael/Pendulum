using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyBullet : MonoBehaviour
{
    [Tooltip("Writes in the Console what the bullet touches (for testing)")]
    public bool debugHits = false;

    int damage = 1;
    float range;
    float travelled;
    Vector3 velocity;
    Rigidbody rb;

    public void Launch(Vector3 direction, float speed, float size, float range, int damage)
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        this.damage = damage;
        this.range = range;
        velocity = direction.normalized * speed;

        transform.localScale = Vector3.one * size;
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

        float step = velocity.magnitude * Time.fixedDeltaTime;
        travelled += step;
        if (travelled >= range)
        {
            Destroy(gameObject);
            return;
        }

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

    // The player
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PersistentPlayer>() == null) return;

        if (debugHits) Debug.Log("Enemy bullet hit the player");

        Health health = FindAnyObjectByType<Health>();
        if (health != null) health.Remove(damage);

        Destroy(gameObject);
    }
}