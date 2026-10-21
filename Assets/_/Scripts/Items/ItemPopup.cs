using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ItemPopup : MonoBehaviour
{
    // Each floor has its own UI, so each floor's popup registers itself here
    public static ItemPopup Instance { get; private set; }

    [Header("Parts")]
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    [Tooltip("Optional: shows the item's icon")]
    public Image iconImage;
    [Tooltip("Optional: dark panel behind the texts")]
    public Image background;

    [Header("Layout")]
    [Tooltip("Places the background, icon and texts inside the banner when the game starts (no Rect Transform work needed)")]
    public bool autoLayout = true;
    public float padding = 20f;
    public float iconSize = 100f;
    [Tooltip("Share of the banner's height used by the name. The description gets the rest.")]
    [Range(0.2f, 0.7f)] public float nameShare = 0.4f;

    [Header("Timing (seconds)")]
    public float fadeInTime = 0.2f;
    public float showTime = 3f;
    public float fadeOutTime = 0.5f;

    CanvasGroup group;
    float timer;
    bool showing;
    float nameRowHeight = -1f; // measured from the text when an item is shown

    void Awake()
    {
        Instance = this;
        group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        if (autoLayout) ApplyLayout();
    }

    // Measures the texts and sets the banner's height to match
    void FitHeightToText(ItemData item)
    {
        RectTransform rect = (RectTransform)transform;

        float textLeft = iconImage != null ? padding * 2f + iconSize : padding;
        float textWidth = Mathf.Max(10f, rect.rect.width - textLeft - padding);

        float nameHeight = nameText != null
            ? nameText.GetPreferredValues(item.itemName, textWidth, 0f).y : 0f;
        float descriptionHeight = descriptionText != null
            ? descriptionText.GetPreferredValues(item.description, textWidth, 0f).y : 0f;

        nameRowHeight = nameHeight;

        // At least tall enough for the icon
        float height = Mathf.Max(iconSize + padding * 2f, padding * 2f + nameHeight + descriptionHeight);
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
    }

    // Puts every part in its place inside the banner
    void ApplyLayout()
    {
        // Background: fills the whole banner
        if (background != null)
        {
            RectTransform r = background.rectTransform;
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
        }

        // Icon: on the left, vertically centered
        float textLeft = padding;
        if (iconImage != null)
        {
            RectTransform r = iconImage.rectTransform;
            r.anchorMin = new Vector2(0f, 0.5f);
            r.anchorMax = new Vector2(0f, 0.5f);
            r.pivot = new Vector2(0f, 0.5f);
            r.sizeDelta = new Vector2(iconSize, iconSize);
            r.anchoredPosition = new Vector2(padding, 0f);
            iconImage.preserveAspect = true;
            textLeft = padding * 2f + iconSize;
        }

        float nameHeight = nameRowHeight >= 0f
            ? nameRowHeight
            : ((RectTransform)transform).rect.height * nameShare;

        // Name: top row, to the right of the icon
        if (nameText != null)
        {
            RectTransform r = nameText.rectTransform;
            r.anchorMin = new Vector2(0f, 1f);
            r.anchorMax = new Vector2(1f, 1f);
            r.offsetMin = new Vector2(textLeft, -(padding + nameHeight));
            r.offsetMax = new Vector2(-padding, -padding);
            nameText.alignment = TextAlignmentOptions.MidlineLeft;
        }

        // Description: the rest of the banner, wraps onto several lines
        if (descriptionText != null)
        {
            RectTransform r = descriptionText.rectTransform;
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(textLeft, padding);
            r.offsetMax = new Vector2(-padding, -(padding + nameHeight));
            descriptionText.alignment = TextAlignmentOptions.TopLeft;
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // Called by PlayerInventory when an item is picked up
    public static void Show(ItemData item)
    {
        if (Instance != null) Instance.Display(item);
    }

    void Display(ItemData item)
    {
        if (item == null) return;

        if (nameText != null) nameText.text = item.itemName;
        if (descriptionText != null) descriptionText.text = item.description;

        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = item.icon != null;
        }

        timer = 0f;
        // The banner grows (downward) to fit the text, so long descriptions never bleed out
        if (autoLayout)
        {
            FitHeightToText(item);
            ApplyLayout();
        }

        showing = true;
    }

    void Update()
    {
        if (!showing) return;

        timer += Time.unscaledDeltaTime;

        float total = fadeInTime + showTime + fadeOutTime;
        if (timer >= total)
        {
            showing = false;
            group.alpha = 0f;
            return;
        }

        if (timer < fadeInTime)
            group.alpha = fadeInTime > 0f ? timer / fadeInTime : 1f;
        else if (timer < fadeInTime + showTime)
            group.alpha = 1f;
        else
            group.alpha = fadeOutTime > 0f ? 1f - (timer - fadeInTime - showTime) / fadeOutTime : 0f;
    }
}