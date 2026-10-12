using StarterAssets;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public void Launch(Vector3 direction, float speed, float size, float range)
    {
        transform.localScale = Vector3.one * size;

        GetComponent<Rigidbody>().linearVelocity = direction * speed;

        Destroy(gameObject, range / speed); // lifetime = distance / speed
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return; // ignore trigger zones
        if (other.GetComponentInParent<ThirdPersonController>() != null) return;
        Destroy(gameObject);
    }
}