using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyShooter : MonoBehaviour
{
    [Header("Bullets")]
    public EnemyBullet bulletPrefab;
    public int bulletDamage = 1;
    public float bulletSpeed = 6f;
    public float bulletSize = 0.4f;
    [Tooltip("Distance in units before the bullet disappears")]
    public float bulletRange = 20f;
    [Tooltip("Height of the bullets above the enemy's origin (the player's body is around 1)")]
    public float bulletHeight = 1f;
    [Tooltip("Optional: bullets come out of this point instead")]
    public Transform muzzle;

    [Header("Firing")]
    [Tooltip("Shots per second (0.5 = one shot every 2 seconds)")]
    public float fireRate = 0.7f;
    [Tooltip("Bullets in each shot")]
    public int bulletsPerShot = 1;
    [Tooltip("Total angle of the fan when there are several bullets per shot (degrees)")]
    public float spreadAngle = 30f;
    [Tooltip("Random aiming error (degrees). 0 = perfect aim.")]
    public float inaccuracy = 0f;
    [Tooltip("Seconds before the first shot once the enemy wakes up")]
    public float startDelay = 1f;

    [Header("Detection")]
    [Tooltip("Only shoots when the player is this close. 0 = always shoots.")]
    public float aggroRange = 0f;
    [Tooltip("Only shoots when no wall is between the enemy and the player")]
    public bool requireLineOfSight = false;

    [Header("Hitbox")]
    [Tooltip("Automatically make the Box Collider tall so bullets can't fly over it")]
    public bool fitHitbox = true;
    public float hitboxCenterY = 1f;
    public float hitboxHeight = 3f;

    [Header("Sprite (same idea as EnemyChaser)")]
    [Tooltip("Child sprite that always faces the camera (optional)")]
    public Transform sprite;
    public float spriteHeight = 1f;
    public float spriteScreenUp = 0.4f;

    EnemyHealth health;
    Transform player;
    float nextShotTime;
    bool snappedToPlayerHeight;

    void Awake()
    {
        health = GetComponent<EnemyHealth>();

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

    // Also runs when the room wakes the enemy up
    void OnEnable()
    {
        nextShotTime = Time.time + startDelay;
    }

    void Update()
    {
        if (health.IsDead) return;      // dying: no more shots
        if (!FindPlayer()) return;

        // Same floor height as the player, so the sprite and the bullets line up
        if (!snappedToPlayerHeight)
        {
            Vector3 p = transform.position;
            p.y = player.position.y;
            transform.position = p;
            snappedToPlayerHeight = true;
        }

        // Chronomètre Cassé: frozen, and the shot timer waits too
        if (EnemyChaser.IsFrozen)
        {
            nextShotTime += Time.deltaTime;
            return;
        }

        if (Time.time < nextShotTime) return;

        Vector3 origin = muzzle != null ? muzzle.position : transform.position + Vector3.up * bulletHeight;

        Vector3 flat = player.position - transform.position;
        flat.y = 0f;
        if (flat.sqrMagnitude < 0.0001f) return;

        if (aggroRange > 0f && flat.magnitude > aggroRange) return;
        if (requireLineOfSight && !CanSeePlayer(origin)) return;

        Shoot(origin, flat.normalized);
        nextShotTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
    }

    void LateUpdate()
    {
        Camera cam = Camera.main;
        if (sprite == null || cam == null) return;

        sprite.rotation = cam.transform.rotation;
        Vector3 screenUp = Vector3.ProjectOnPlane(cam.transform.up, Vector3.up).normalized;
        sprite.position = transform.position + Vector3.up * spriteHeight + screenUp * spriteScreenUp;
    }

    void Shoot(Vector3 origin, Vector3 aimDirection)
    {
        if (bulletPrefab == null) return;

        int count = Mathf.Max(1, bulletsPerShot);
        for (int i = 0; i < count; i++)
        {
            // Bullets are spread evenly across the fan, centered on the player
            float angle = count == 1
                ? 0f
                : Mathf.Lerp(-spreadAngle * 0.5f, spreadAngle * 0.5f, i / (float)(count - 1));
            angle += Random.Range(-inaccuracy, inaccuracy);

            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * aimDirection;

            EnemyBullet bullet = Instantiate(bulletPrefab, origin, Quaternion.LookRotation(direction));
            bullet.Launch(direction, bulletSpeed, bulletSize, bulletRange, bulletDamage);
        }
    }

    bool CanSeePlayer(Vector3 origin)
    {
        Vector3 target = player.position + Vector3.up * bulletHeight;
        Vector3 toTarget = target - origin;

        if (!Physics.Raycast(origin, toTarget.normalized, out RaycastHit hit, toTarget.magnitude,
                             ~0, QueryTriggerInteraction.Ignore))
            return true; // nothing in the way

        return hit.collider.GetComponentInParent<PersistentPlayer>() != null;
    }

    bool FindPlayer()
    {
        if (player != null) return true;
        if (PersistentPlayer.Instance == null) return false;

        // The PersistentPlayer root doesn't move: the CharacterController's object does.
        CharacterController cc = PersistentPlayer.Instance.GetComponentInChildren<CharacterController>();
        player = cc != null ? cc.transform : PersistentPlayer.Instance.transform;
        return true;
    }
}