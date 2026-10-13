using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyChaser : MonoBehaviour
{
    public float moveSpeed = 2f;
    public int contactDamage = 10;
    public float contactCooldown = 1f;

    [Tooltip("Enemy stays idle until the player is this close. 0 = always chasing.")]
    public float aggroRange = 0f;

    [Tooltip("Child sprite that always faces the camera (optional)")]
    public Transform sprite;

    Rigidbody rb;
    Health playerHealth;
    Transform player;
    float nextHitTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
    }

    void FixedUpdate()
    {
        if (!FindPlayer()) return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        if (aggroRange > 0f && toPlayer.magnitude > aggroRange)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        rb.linearVelocity = toPlayer.normalized * moveSpeed;
    }

    void LateUpdate()
    {
        if (sprite != null && Camera.main != null)
            sprite.rotation = Camera.main.transform.rotation;
    }

    bool FindPlayer()
    {
        if (player != null) return true;
        if (PersistentPlayer.Instance == null) return false;

        player = PersistentPlayer.Instance.transform;
        playerHealth = FindAnyObjectByType<Health>();
        return true;
    }

    void OnCollisionStay(Collision col) => TryHurt(col.collider);
    void OnTriggerStay(Collider other) => TryHurt(other);

    void TryHurt(Collider other)
    {
        if (Time.time < nextHitTime) return;
        if (PersistentPlayer.Instance == null) return;
        if (other.GetComponentInParent<PersistentPlayer>() == null) return;

        if (playerHealth == null) playerHealth = FindAnyObjectByType<Health>();
        if (playerHealth == null) return;

        playerHealth.Remove(contactDamage);
        nextHitTime = Time.time + contactCooldown;
    }
}