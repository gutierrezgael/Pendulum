using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Junior : MonoBehaviour
{
    [Tooltip("1 = same speed as Cloche. Junior copies Cloche's movement, multiplied by this.")]
    public float speedMultiplier = 1.8f;

    [Header("Attack")]
    public int contactDamage = 1;
    public float hitInterval = 0.5f;
    [Tooltip("Reach around Junior (added to the enemy's hitbox)")]
    public float hitRadius = 0.8f;

    [Header("Sprite (drawn over the controller)")]
    [Tooltip("Child object with a SpriteRenderer")]
    public Transform sprite;
    public float spriteHeight = 1f;
    public float spriteScreenUp = 0.4f;

    [Header("Animation")]
    [Tooltip("The 4 frames of Junior")]
    public Sprite[] frames;
    public float framesPerSecond = 8f;
    [Tooltip("Also play the animation when standing still")]
    public bool animateWhileIdle = false;
    [Tooltip("Mirror the sprite when Junior moves toward the left of the screen")]
    public bool flipWithDirection = false;

    CharacterController cc;
    CharacterController playerCC;
    Transform body;
    SpriteRenderer spriteRenderer;
    float animTimer;
    readonly Dictionary<EnemyHealth, float> nextHit = new Dictionary<EnemyHealth, float>();

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (sprite != null) spriteRenderer = sprite.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // The player is gone (game over, back to menu): so is Junior
        if (PersistentPlayer.Instance == null)
        {
            Destroy(gameObject);
            return;
        }

        if (body == null && !FindBody()) return;

        if (!cc.enabled) return; // never move a disabled controller

        // Copy the player's real movement (not teleports or room slides), but faster
        Vector3 move = playerCC.velocity;
        move.y = 0f;
        move *= speedMultiplier * Time.deltaTime;
        move.y = body.position.y - transform.position.y; // same floor height as the player
        cc.Move(move);

        HitEnemies();
    }

    void LateUpdate()
    {
        Camera cam = Camera.main;
        if (sprite == null || cam == null) return;

        // Draw the sprite over the controller, facing the camera
        sprite.rotation = cam.transform.rotation;
        Vector3 screenUp = Vector3.ProjectOnPlane(cam.transform.up, Vector3.up).normalized;
        sprite.position = transform.position + Vector3.up * spriteHeight + screenUp * spriteScreenUp;

        Animate(cam);
    }

    void Animate(Camera cam)
    {
        if (spriteRenderer == null || frames == null || frames.Length == 0) return;

        Vector3 flatVelocity = cc.velocity;
        flatVelocity.y = 0f;
        bool moving = flatVelocity.sqrMagnitude > 0.01f;

        if (moving || animateWhileIdle)
        {
            animTimer += Time.deltaTime * framesPerSecond;
            spriteRenderer.sprite = frames[(int)animTimer % frames.Length];
        }
        else
        {
            animTimer = 0f;
            spriteRenderer.sprite = frames[0];
        }

        if (flipWithDirection && moving)
        {
            Vector3 screenRight = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
            float side = Vector3.Dot(flatVelocity, screenRight);
            if (Mathf.Abs(side) > 0.01f) spriteRenderer.flipX = side < 0f;
        }
    }

    bool FindBody()
    {
        foreach (CharacterController c in PersistentPlayer.Instance.GetComponentsInChildren<CharacterController>(true))
        {
            if (c == cc) continue;
            playerCC = c;
            body = c.transform;
            Physics.IgnoreCollision(cc, playerCC); // they don't block each other
            return true;
        }
        return false;
    }

    void HitEnemies()
    {
        foreach (Collider c in Physics.OverlapSphere(transform.position, hitRadius))
        {
            EnemyHealth enemy = c.GetComponentInParent<EnemyHealth>();
            if (enemy == null) continue;

            if (nextHit.TryGetValue(enemy, out float readyTime) && Time.time < readyTime) continue;

            nextHit[enemy] = Time.time + hitInterval;
            enemy.TakeDamage(contactDamage);
        }
    }
}