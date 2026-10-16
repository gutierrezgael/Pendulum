using TMPro;
using UnityEngine;
using UnityEngine.Events;
public class ItemPickup : MonoBehaviour
{
    [Header("What it offers")]
    [Tooltip("Leave empty to pick randomly from the pool")]
    public ItemData item;
    public ItemPool pool;

    [Header("Appearing")]
    [Tooltip("Hidden until the player enters the room (needs a RoomController on a parent and a CES zone calling StartRoom)")]
    public bool hideUntilRoomEntered = true;
    [Tooltip("Seconds the item takes to pop in")]
    public float appearTime = 0.3f;

    [Header("Display")]
    [Tooltip("Child SpriteRenderer that shows the item's icon (faces the camera)")]
    public SpriteRenderer icon;
    public float spriteHeight = 1f;
    public float spriteScreenUp = 0.4f;
    [Tooltip("Optional text showing the item's name")]
    public TMP_Text nameLabel;
    [Tooltip("Optional text showing the item's description")]
    public TMP_Text descriptionLabel;

    [Header("Floating (up and down on the screen)")]
    public float bobHeight = 0.25f;
    public float bobSpeed = 2f;

    [Header("Events")]
    public UnityEvent onPickedUp;

    RoomController room;
    Vector3 iconBaseScale = Vector3.one;
    bool revealed;
    bool taken;
    float appearTimer;

    void Start()
    {
        if (item == null && pool != null)
            item = pool.PickRandom(FindAnyObjectByType<PlayerInventory>());

        // Nothing to offer (empty pool, or everything already owned)
        if (item == null)
        {
            gameObject.SetActive(false);
            return;
        }

        if (icon != null)
        {
            icon.sprite = item.icon;
            iconBaseScale = icon.transform.localScale;
        }
        if (nameLabel != null) nameLabel.text = item.itemName;
        if (descriptionLabel != null) descriptionLabel.text = item.description;

        room = GetComponentInParent<RoomController>();
        if (hideUntilRoomEntered && room != null)
        {
            SetVisible(false);
            room.onPlayerEntered.AddListener(Reveal);
        }
        else
        {
            revealed = true;
            appearTimer = appearTime; // already fully shown
        }
    }

    void OnDestroy()
    {
        if (room != null) room.onPlayerEntered.RemoveListener(Reveal);
    }

    // Called by the room every time the player enters it
    void Reveal()
    {
        if (revealed) return;
        revealed = true;
        appearTimer = 0f;
        SetVisible(true);

        if (room != null) room.onPlayerEntered.RemoveListener(Reveal);
    }

    void SetVisible(bool visible)
    {
        if (icon != null) icon.gameObject.SetActive(visible);
        if (nameLabel != null) nameLabel.gameObject.SetActive(visible);
        if (descriptionLabel != null) descriptionLabel.gameObject.SetActive(visible);

        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = visible;
    }

    void LateUpdate()
    {
        Camera cam = Camera.main;
        if (icon == null || cam == null || !revealed) return;

        // Pop-in: grows from nothing, fast then slow
        appearTimer += Time.deltaTime;
        float k = appearTime > 0f ? Mathf.Clamp01(appearTimer / appearTime) : 1f;
        k = 1f - (1f - k) * (1f - k);
        icon.transform.localScale = iconBaseScale * k;

        // Face the camera and float up and down on the screen
        icon.transform.rotation = cam.transform.rotation;
        Vector3 screenUp = Vector3.ProjectOnPlane(cam.transform.up, Vector3.up).normalized;
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        icon.transform.position = transform.position + Vector3.up * spriteHeight + screenUp * (spriteScreenUp + bob);
    }

    void OnTriggerEnter(Collider other)
    {
        if (taken || item == null || !revealed) return;

        PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();
        if (inventory == null) return;

        taken = true;
        inventory.Acquire(item);
        onPickedUp.Invoke();
        Destroy(gameObject);
    }
}