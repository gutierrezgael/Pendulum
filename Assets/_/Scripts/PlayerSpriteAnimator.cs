using UnityEngine;

public class PlayerSpriteAnimator : MonoBehaviour
{
    public Sprite[] upFrames;    // 4 frames, facing up
    public Sprite[] downFrames;  // 4 frames, facing down
    public float framesPerSecond = 8f;
    public float moveThreshold = 0.02f; // min movement per frame to count as moving

    SpriteRenderer sr;
    Transform body;      // the capsule that actually moves
    Vector3 lastPos;
    bool facingUp;       // remembers the last vertical direction
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

        // Movement since last frame
        Vector3 delta = body.position - lastPos;
        lastPos = body.position;
        delta.y = 0f;

        bool moving = delta.magnitude > moveThreshold * Time.deltaTime * 60f;

        if (moving)
        {
            // How much of the movement is "up the screen"?
            Vector3 screenUp = Vector3.ProjectOnPlane(cam.transform.up, Vector3.up).normalized;
            float vertical = Vector3.Dot(delta, screenUp);

            // Only change direction on real up/down movement; pure left/right keeps the last one
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
            sr.sprite = frames[0]; // idle = first frame
        }
    }
}