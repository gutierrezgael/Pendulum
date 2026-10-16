using StarterAssets;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Tooltip("Writes in the Console what the bullet touches (for testing)")]
    public bool debugHits = false;

    [Tooltip("Goes through enemies instead of disappearing (the Engrenage Perdu)")]
    public bool pierceEnemies = false;

    int damage = 1;
    Vector3 flightDirection;
    float flightSpeed;

    public void Launch(Vector3 direction, float speed, float size, float range, int damage)
    {
        this.damage = damage;
        flightDirection = direction.normalized;
        flightSpeed = speed;

        transform.localScale = Vector3.one * size;

        GetComponent<Rigidbody>().linearVelocity = direction * speed;

        Destroy(gameObject, range / speed); // lifetime = distance / speed
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