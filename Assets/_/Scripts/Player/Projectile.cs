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

    [Tooltip("Degrees per second the sprite spins (0 = none). Nice for the Engrenage Perdu.")]
    public float spinSpeed = 0f;

    [Header("End of flight")]
    [Tooltip("Seconds before the maximum range during which the bullet shrinks to nothing")]
    public float shrinkDuration = 0.2f;

    [System.Serializable]
    public struct SizePitch
    {
        [Tooltip("This pitch is used for bullets of this size or bigger, until the next step")]
        public float fromSize;
        [Tooltip("1 = normal, 2 = an octave higher, 0.5 = an octave lower")]
        public float pitch;
    }

    [Tooltip("Pitch of the shot depending on the bullet's size. The step with the biggest 'From Size' that fits is used.")]
    public SizePitch[] pitchBySize =
    {
        new SizePitch { fromSize = 0f,   pitch = 1.3f  },  // very small: higher
        new SizePitch { fromSize = 0.4f, pitch = 1.15f },
        new SizePitch { fromSize = 0.7f, pitch = 1f    },  // normal
        new SizePitch { fromSize = 1.2f, pitch = 0.85f },
        new SizePitch { fromSize = 2f,   pitch = 0.7f  },  // very big: lower
    };

    [Tooltip("Safety: an invisible bullet is removed after this many seconds even if its sound never ends")]
    public float maxSoundWait = 5f;

    int damage = 1;
    Vector3 flightDirection;
    float flightSpeed;
    float lifetime;
    float age;
    float spinAngle;
    Vector3 baseScale = Vector3.one;

    bool despawning;      // the bullet is over: invisible, waiting for its sound to end
    float despawnTime;
    AudioSource ownSource;

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

        PlayShotSound(size);
    }

    // Big bullets sound lower, small ones higher
    void PlayShotSound(float size)
    {
        float pitch = PitchFor(size);

        ownSource = GetComponent<AudioSource>();
        if (ownSource == null) return;

        ownSource.pitch = pitch;
        ownSource.Play();
    }

    float PitchFor(float size)
    {
        float pitch = 1f;
        float bestFromSize = float.NegativeInfinity;
        foreach (SizePitch step in pitchBySize)
        {
            if (size >= step.fromSize && step.fromSize >= bestFromSize)
            {
                bestFromSize = step.fromSize;
                pitch = step.pitch;
            }
        }
        return pitch;
    }

    // The bullet is over (range, wall, enemy). Its sound is never cut off:
    // everything visible and solid is switched off, and the object only disappears
    // once its Audio Source has stopped.
    void Despawn()
    {
        if (despawning) return;
        despawning = true;
        despawnTime = Time.time;

        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;

        Rigidbody body = GetComponent<Rigidbody>();
        body.linearVelocity = Vector3.zero;
        body.isKinematic = true;
    }

    void Update()
    {
        if (despawning)
        {
            // Invisible: waiting for the sound to end
            if (ownSource == null || !ownSource.isPlaying || Time.time - despawnTime > maxSoundWait)
                Destroy(gameObject);
            return;
        }

        if (flightSpeed <= 0f) return;

        age += Time.deltaTime;
        float remaining = lifetime - age;
        if (remaining <= 0f)
        {
            Despawn();
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
        {
            spinAngle += spinSpeed * Time.deltaTime;
            sprite.rotation = cam.transform.rotation * Quaternion.Euler(0f, 0f, spinAngle);
        }
    }

    // Walls: a ray along the flight direction. Unlike the bullet's own collider, it never
    // touches the floor, so big bullets don't vanish the moment they are fired.
    void FixedUpdate()
    {
        if (despawning || flightSpeed <= 0f) return;

        float reach = transform.localScale.x * 0.5f + flightSpeed * Time.fixedDeltaTime;
        if (!Physics.Raycast(transform.position, flightDirection, out RaycastHit hit, reach,
                             ~0, QueryTriggerInteraction.Ignore)) return;

        Collider other = hit.collider;
        if (other.GetComponentInParent<EnemyHealth>() != null) return;       // enemies are handled below
        if (other.GetComponentInParent<ThirdPersonController>() != null) return;
        if (other.GetComponentInParent<PersistentPlayer>() != null) return;
        if (other.GetComponentInParent<Junior>() != null) return;

        if (debugHits) Debug.Log($"Bullet stopped by '{other.name}'");
        Despawn();
    }

    // Enemies: damage on contact
    void OnTriggerEnter(Collider other)
    {
        if (despawning) return;

        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy == null) return;

        if (debugHits) Debug.Log($"Bullet hit enemy '{enemy.name}'");

        enemy.TakeDamage(damage);
        if (!pierceEnemies) Despawn();
    }
}