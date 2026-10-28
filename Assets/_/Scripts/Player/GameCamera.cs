using UnityEngine;
using UnityEngine.SceneManagement;

public class GameCamera : MonoBehaviour
{
    // The camera lives with the persistent player: rooms in any floor can reach it from here
    public static GameCamera Instance { get; private set; }

    [Header("Mode")]
    [Tooltip("Follows the player from the start of the game")]
    public bool startFollowing = false;

    [Header("Height")]
    [Tooltip("Camera height. 0 = keeps the height the camera has when the game starts.")]
    public float height = 0f;

    [Header("Following the player")]
    [Tooltip("Shifts the camera away from the player (X and Z)")]
    public Vector2 followOffset = Vector2.zero;
    [Tooltip("How smoothly it follows (seconds). 0 = glued to the player.")]
    public float followSmoothTime = 0.15f;

    [Header("Switching")]
    [Tooltip("Seconds the camera takes to glide when it changes room or starts following")]
    public float glideTime = 0.4f;

    bool registered;
    bool following;
    bool hasStaticTarget;
    Vector3 staticTarget;
    Vector3 velocity;
    float transitionUntil;
    bool snapNext = true; // no glide when the game or a new floor starts
    Transform player;

    public bool IsFollowing => following;

    void Awake()
    {
        // A second copy of the player (new floor) is destroyed by PersistentPlayer: ignore it
        if (Instance != null && Instance != this)
        {
            enabled = false;
            return;
        }

        Instance = this;
        registered = true;
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (height <= 0f) height = transform.position.y;
        following = startFollowing;
    }

    void OnDestroy()
    {
        if (!registered) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this) Instance = null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        snapNext = true; // new floor: the camera jumps to its place instead of gliding there
        player = null;
    }

    // --- Call these from a CES (Invoke Events) ---

    public void StartFollowing() => SetFollowing(true);
    public void StopFollowing() => SetFollowing(false);
    public void ToggleFollowing() => SetFollowing(!following);

    void SetFollowing(bool value)
    {
        if (following == value) return;
        following = value;
        hasStaticTarget = false; // stopping: the camera stays where it is
        transitionUntil = Time.time + glideTime;
    }

    // --- Called by RoomController (or anything else): stay still, centered on this position ---

    public void GoStatic(Vector3 position)
    {
        following = false;
        hasStaticTarget = true;
        staticTarget = position;
        transitionUntil = Time.time + glideTime;
    }

    void LateUpdate()
    {
        Vector3 goal;
        float smooth;

        if (following)
        {
            if (!FindPlayer()) return;
            goal = new Vector3(player.position.x + followOffset.x, height, player.position.z + followOffset.y);
            smooth = Time.time < transitionUntil ? glideTime : followSmoothTime;
        }
        else if (hasStaticTarget)
        {
            goal = new Vector3(staticTarget.x, height, staticTarget.z);
            smooth = glideTime;
        }
        else
        {
            return; // standing still with no target: a CES Transform action may move the camera
        }

        if (snapNext)
        {
            transform.position = goal;
            velocity = Vector3.zero;
            snapNext = false;
            return;
        }

        transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, Mathf.Max(0.0001f, smooth));
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
