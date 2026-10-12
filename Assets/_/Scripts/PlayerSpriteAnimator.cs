using UnityEngine;

public class PlayerSpriteAnimator : MonoBehaviour
{
    public Sprite[] upFrames;    // 4 frames, facing up
    public Sprite[] downFrames;  // 4 frames, facing down
    public float framesPerSecond = 8f;
    public float moveThreshold = 0.02f;

    [Header("Position")]
    public float heightOffset = 1f;    // sprite height above the body's origin
    public float screenUpOffset = 0.4f; // pushes the sprite up the screen so the feet sit on the hitbox

    SpriteRenderer sr;
    Transform body;
    Vector3 lastPos;
    bool facingUp;
    float animTimer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        body = transform.parent;
        lastPos = body.position;
    }

    void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Always face the camera, ignoring the capsule's rotation
        transform.rotation = cam.transform.rotation;

        // "Up the screen" on the floor, declared once and used below
        Vector3 screenUp = Vector3.ProjectOnPlane(cam.transform.up, Vector3.up).normalized;

        // World-space position, so it doesn't spin with the capsule
        transform.position = body.position + Vector3.up * heightOffset + screenUp * screenUpOffset;

        // Movement since last frame
        Vector3 delta = body.position - lastPos;
        lastPos = body.position;
        delta.y = 0f;

        bool moving = delta.magnitude > moveThreshold * Time.deltaTime * 60f;

        if (moving)
        {
            float vertical = Vector3.Dot(delta, screenUp);

            if (Mathf.Abs(vertical) > 0.001f)
                facingUp = vertical > 0f;
        }

        Sprite[] frames = facingUp ? upFrames : downFrames;
        if (frames == null || frames.Length == 0) return;

        if (moving)
        {
            animTimer += Time.deltaTime * framesPerSecond;
            sr.sprite = frames[(int)animTimer % frames.Length];
        }
        else
        {
            animTimer = 0f;
            sr.sprite = frames[0];
        }
    }
}