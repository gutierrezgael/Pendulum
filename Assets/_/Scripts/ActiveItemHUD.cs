using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ActiveItemHUD : MonoBehaviour
{
    [Header("Parts")]
    [Tooltip("Child holding the icon, overlay and texts. Hidden while the player holds no active item. The box itself must NOT be inside it.")]
    public GameObject content;
    [Tooltip("Shows the icon of the held item")]
    public Image iconImage;
    [Tooltip("Image over the icon, Image Type = Filled. It empties as the cooldown runs out.")]
    public Image cooldownOverlay;
    [Tooltip("Shows: frames since last use / frames needed to use it again")]
    public TMP_Text cooldownText;
    [Tooltip("Optional: shows the key to press")]
    public TMP_Text keyText;

    [Header("Counter")]
    [Tooltip("The cooldown is in seconds: this converts it to frames (seconds x this number)")]
    public float framesPerSecond = 60f;
    [Tooltip("{0} = frames since last use, {1} = frames needed to use it again")]
    public string format = "{0}/{1}";

    [Header("Layout")]
    [Tooltip("Puts the counter right under the box when the game starts (no manual placement needed)")]
    public bool placeCounterUnderBox = true;
    [Tooltip("Gap in pixels between the box and the counter")]
    public float counterGap = 8f;

    PlayerInventory inventory;

    void Start()
    {
        if (!placeCounterUnderBox || cooldownText == null) return;

        // Anchored to the bottom edge of its parent (the box), hanging downward
        RectTransform text = cooldownText.rectTransform;
        RectTransform parent = text.parent as RectTransform;

        text.anchorMin = new Vector2(0.5f, 0f);
        text.anchorMax = new Vector2(0.5f, 0f);
        text.pivot = new Vector2(0.5f, 1f);
        text.anchoredPosition = new Vector2(0f, -counterGap);
        text.sizeDelta = new Vector2(parent != null ? parent.rect.width : 100f, 30f);
        cooldownText.alignment = TextAlignmentOptions.Center;
    }

    void Update()
    {
        // The player lives from floor to floor: find it again if needed
        if (inventory == null) inventory = FindAnyObjectByType<PlayerInventory>();

        ItemData item = inventory != null ? inventory.activeItem : null;

        if (content != null) content.SetActive(item != null);
        if (item == null) return;

        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = item.icon != null;
        }

        float left = inventory.ActiveCooldownLeft;
        float total = Mathf.Max(0.01f, inventory.ActiveCooldownTotal);

        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = Mathf.Clamp01(left / total);

        if (cooldownText != null)
        {
            // Ready (or never used): shows a full counter, like 120/120
            int totalFrames = Mathf.RoundToInt(total * framesPerSecond);
            int sinceUse = Mathf.Clamp(Mathf.RoundToInt((total - left) * framesPerSecond), 0, totalFrames);
            cooldownText.text = string.Format(format, sinceUse, totalFrames);
        }

        if (keyText != null)
            keyText.text = KeyName(inventory.useActiveKey);
    }

    static string KeyName(Key key)
    {
        switch (key)
        {
            case Key.Space: return "Espace";
            case Key.Enter: return "Entrée";
            case Key.LeftShift: return "Maj";
            default: return key.ToString();
        }
    }
}