using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyChaser : MonoBehaviour
{
    public float moveSpeed = 2f;

    [Header("Contact damage")]
    public int contactDamage = 10;
    public float contactCooldown = 1f;
    [Tooltip("Distance (on the floor) between enemy and player centers at which the player gets hurt")]
    public float contactRange = 1.5f;

    [Tooltip("Enemy stays idle until the player is this close. 0 = always chasing.")]
    public float aggroRange = 0f;

    [Tooltip("Stops walking when this close to the player (a boss that keeps its distance). 0 = walks right up to her.")]
    public float stopDistance = 0f;

    [Header("Hitbox")]
    [Tooltip("Automatically make the Box Collider tall so bullets can't fly over it")]
    public bool fitHitbox = true;
    public float hitboxCenterY = 1f;
    public float hitboxHeight = 3f;

    [Tooltip("Child sprite that always faces the camera (optional)")]
    public Transform sprite;

    [Header("Sprite position (same idea as PlayerSpriteAnimator)")]
    [Tooltip("Sprite height above the enemy's origin")]
    public float spriteHeight = 1f;
    [Tooltip("Pushes the sprite up the screen so the feet sit on the hitbox")]
    public float spriteScreenUp = 0.4f;

    Rigidbody rb;
    EnemyHealth health;
    SpawnPopIn pop;
    Health playerHealth;
    Transform player;
    float nextHitTime;

    // --- Chronomètre Cassé: every enemy stops (and shooting enemies should check IsFrozen too) ---
    static float frozenUntil;
    public static bool IsFrozen => Time.time < frozenUntil;
    public static void FreezeAll(float seconds) => frozenUntil = Time.time + seconds;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => frozenUntil = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<EnemyHealth>();
        pop = GetComponent<SpawnPopIn>();
        rb.useGravity = false;
        rb.isKinematic = true; // nothing can push it; we move it ourselves

        // The enemy's origin is at the player's feet height, but bullets fly about 1 unit higher.
        // A tall hitbox makes sure they hit whatever the Inspector values were.
        if (fitHitbox)
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box != null)
            {
                Vector3 c = box.center; c.y = hitboxCenterY; box.center = c;
                Vector3 s = box.size; s.y = hitboxHeight; box.size = s;
            }
        }
    }

    void FixedUpdate()
    {
        if (!FindPlayer()) return;
        if (IsFrozen) return; // frozen: no movement, no contact damage
        if (pop != null && pop.IsSpawning) return; // still popping in: no movement, no contact damage
        if (health.IsDead) return; // dying: it stops and can't hurt anymore

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;

        // Chase, and stay at the same floor height as the player
        // (so the sprite and the bullets always line up with the enemy)
        Vector3 newPos = rb.position;
        newPos.y = player.position.y;

        bool aggro = aggroRange <= 0f || dist <= aggroRange;
        if (aggro && dist > Mathf.Max(0.01f, stopDistance))
            newPos += toPlayer.normalized * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(newPos);

        // Contact damage
        if (dist <= contactRange && Time.time >= nextHitTime)
        {
            if (playerHealth == null) playerHealth = FindAnyObjectByType<Health>();
            if (playerHealth != null)
            {
                if (PlayerInvincibility.Hurt(playerHealth, contactDamage))
                    nextHitTime = Time.time + contactCooldown;
            }
        }
    }

    void LateUpdate()
    {
        Camera cam = Camera.main;
        if (sprite == null || cam == null) return;

        // Face the camera, like the player's sprite
        sprite.rotation = cam.transform.rotation;

        // Lift the sprite so a camera-facing sprite doesn't sink into the floor
        Vector3 screenUp = Vector3.ProjectOnPlane(cam.transform.up, Vector3.up).normalized;
        sprite.position = transform.position + Vector3.up * spriteHeight + screenUp * spriteScreenUp;
    }

    bool FindPlayer()
    {
        if (player != null) return true;
        if (PersistentPlayer.Instance == null) return false;

        // The PersistentPlayer root doesn't move: the CharacterController's object does.
        CharacterController cc = PersistentPlayer.Instance.GetComponentInChildren<CharacterController>();
        player = cc != null ? cc.transform : PersistentPlayer.Instance.transform;
        playerHealth = FindAnyObjectByType<Health>();
        return true;
    }
}