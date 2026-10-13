using StarterAssets;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Tooltip("Writes in the Console what the bullet touches (untick when everything works)")]
    public bool debugHits = true;

    int damage = 1;

    public void Launch(Vector3 direction, float speed, float size, float range, int damage)
    {
        this.damage = damage;
        transform.localScale = Vector3.one * size;

        GetComponent<Rigidbody>().linearVelocity = direction * speed;

        Destroy(gameObject, range / speed); // lifetime = distance / speed
    }

    void OnTriggerEnter(Collider other)
    {
        if (debugHits)
            Debug.Log($"Bullet touched '{other.name}' (layer {LayerMask.LayerToName(other.gameObject.layer)}, trigger: {other.isTrigger}) at height {transform.position.y:F2}");

        // Enemies first, so they get hit even if their collider is set as a trigger
        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (other.isTrigger) return; // ignore trigger zones
        if (other.GetComponentInParent<ThirdPersonController>() != null) return;

        Destroy(gameObject);
    }
}