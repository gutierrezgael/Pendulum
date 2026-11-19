using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ColliderEventSystem;

public class ShopUpgrade : MonoBehaviour
{
    [Header("What it sells")]
    public UpgradeData upgrade;

    [Header("Money")]
    [Tooltip("The Coins object of the scene (the one with the Resource script)")]
    public Resource coins;

    [Header("Display (all optional)")]
    [Tooltip("The progress bar Image")]
    public Image progressBar;
    [Tooltip("One sprite per level, in order: nothing bought, 1 bought, 2 bought... up to the maximum")]
    public Sprite[] progressFrames;
    [Tooltip("Shows the price of the next level")]
    public TMP_Text priceText;
    [Tooltip("Shown instead of the price once everything is bought")]
    public string maxText = "MAX";
    [Tooltip("The Buy button: switched off once everything is bought")]
    public Button buyButton;

    [Header("Events (sounds, effects...)")]
    public UnityEvent onPurchased;
    [Tooltip("Not enough coins")]
    public UnityEvent onRefused;
    [Tooltip("Already at the maximum level")]
    public UnityEvent onMaxed;

    // Keeps the picture and the price in sync, even if the variable is cleared or changed elsewhere
    void Update()
    {
        if (upgrade == null) return;

        if (progressBar != null && progressFrames != null && progressFrames.Length > 0)
            progressBar.sprite = progressFrames[Mathf.Clamp(upgrade.Level, 0, progressFrames.Length - 1)];

        if (priceText != null)
            priceText.text = upgrade.IsMaxed ? maxText : upgrade.NextPrice.ToString();

        if (buyButton != null)
            buyButton.interactable = !upgrade.IsMaxed;
    }

    public void Buy()
    {
        if (upgrade == null || upgrade.level == null || coins == null) return;

        if (upgrade.IsMaxed)
        {
            onMaxed.Invoke();
            return;
        }

        int price = upgrade.NextPrice;
        if (coins.Amount < price)
        {
            onRefused.Invoke();
            return;
        }

        coins.Spend(price);
        upgrade.level.RuntimeValue = upgrade.Level + 1;
        onPurchased.Invoke();
    }
}
