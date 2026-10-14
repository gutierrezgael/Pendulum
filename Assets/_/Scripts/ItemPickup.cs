using TMPro;
using UnityEngine;
using UnityEngine.Events;
public class ItemPickup : MonoBehaviour
{
    [Header("What it offers")]
    [Tooltip("Leave empty to pick randomly from the pool")]
    public ItemData item;
    public ItemPool pool;

    [Header("Display")]
    [Tooltip("Child SpriteRenderer that shows the item's icon (faces the camera)")]
    public SpriteRenderer icon;
    public float spriteHeight = 1f;
    public float spriteScreenUp = 0.4f;
    [Tooltip("Optional text showing the item's name")]
    public TMP_Text nameLabel;
    [Tooltip("Optional text showing the item's description")]
    public TMP_Text descriptionLabel;

    [Header("Events")]
    public UnityEvent onPickedUp;

    bool taken;

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

        if (icon != null) icon.sprite = item.icon;
        if (nameLabel != null) nameLabel.text = item.itemName;
        if (descriptionLabel != null) descriptionLabel.text = item.description;
    }

    void LateUpdate()
    {
        Camera cam = Camera.main;
        if (icon == null || cam == null) return;

        icon.transform.rotation = cam.transform.rotation;
        Vector3 screenUp = Vector3.ProjectOnPlane(cam.transform.up, Vector3.up).normalized;
        icon.transform.position = transform.position + Vector3.up * spriteHeight + screenUp * spriteScreenUp;
    }

    void OnTriggerEnter(Collider other)
    {
        if (taken || item == null) return;

        PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();
        if (inventory == null) return;

        taken = true;
        inventory.Acquire(item);
        onPickedUp.Invoke();
        Destroy(gameObject);
    }
}