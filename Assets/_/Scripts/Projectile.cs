using StarterAssets;
using UnityEngine;

public class Projectile : MonoBehaviour
{
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
        if (other.isTrigger) return; // ignore trigger zones
        if (other.GetComponentInParent<ThirdPersonController>() != null) return;

        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy != null) enemy.TakeDamage(damage);

        Destroy(gameObject);
    }
}