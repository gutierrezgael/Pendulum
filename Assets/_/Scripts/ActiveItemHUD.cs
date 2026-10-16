using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class ActiveItemHUD : MonoBehaviour
{
    [Header("Parts")]
    [Tooltip("Child holding everything visible. Hidden while the player holds no active item.")]
    public GameObject content;
    [Tooltip("Shows the icon of the held item")]
    public Image iconImage;
    [Tooltip("Image over the icon, Image Type = Filled. It empties as the cooldown runs out.")]
    public Image cooldownOverlay;
    [Tooltip("Shows the seconds left, or the ready text")]
    public TMP_Text cooldownText;
    [Tooltip("Optional: shows the key to press")]
    public TMP_Text keyText;

    [Header("Text (in French)")]
    public string readyText = "Prêt";

    PlayerInventory inventory;

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
            cooldownText.text = left > 0f ? Mathf.CeilToInt(left).ToString() : readyText;

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