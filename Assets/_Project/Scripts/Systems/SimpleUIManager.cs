using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimpleUIManager : MonoBehaviour
{
    public TMP_Text goldText;
    public TMP_Text healthText;
    public TMP_Text inventoryText;
    public TMP_Text upgradeText;
    public PlayerHealth playerHealth;

    private RectTransform playerHealthFillRect;

    private void Start()
    {
        if (playerHealth != null)
        {
            CreatePlayerHealthBar();
        }
    }

    private void Update()
    {
        RefreshGold();
        RefreshHealth();
        RefreshInventory();
        RefreshUpgrade();
    }

    private void RefreshGold()
    {
        if (goldText != null)
        {
            int gold = GameManager.Instance != null ? GameManager.Instance.gold : 0;
            goldText.text = $"Gold: {gold}";
        }
    }

    private void RefreshHealth()
    {
        if (healthText != null && playerHealth != null)
        {
            healthText.text = $"Health: {playerHealth.currentHealth}/{playerHealth.maxHealth}";
        }

        if (playerHealthFillRect != null && playerHealth != null)
        {
            float normalizedHealth = playerHealth.maxHealth > 0
                ? (float)playerHealth.currentHealth / playerHealth.maxHealth
                : 0f;
            playerHealthFillRect.anchorMax = new Vector2(Mathf.Clamp01(normalizedHealth), 1f);
        }
    }

    private void RefreshInventory()
    {
        if (inventoryText == null)
        {
            return;
        }

        StringBuilder builder = new StringBuilder("Run inventory:");
        if (InventoryManager.Instance != null)
        {
            foreach (RunInventoryEntry entry in InventoryManager.Instance.RunInventory)
            {
                string itemName = entry.itemData != null ? entry.itemData.displayName : entry.itemId;
                if (entry.itemData != null)
                {
                    builder.Append(
                        $"\n{itemName} [{entry.itemData.rarity}] x{entry.amount} - {entry.itemData.sellPrice} gold each");
                }
                else
                {
                    builder.Append($"\n{itemName} x{entry.amount}");
                }
            }
        }

        inventoryText.text = builder.ToString();
    }

    private void RefreshUpgrade()
    {
        if (upgradeText != null)
        {
            int level = GameManager.Instance != null ? GameManager.Instance.damageUpgradeLevel : 0;
            int bonus = GameManager.Instance != null ? GameManager.Instance.damageBonus : 0;
            upgradeText.text = $"Damage upgrade: {level} (+{bonus})";
        }
    }

    private void CreatePlayerHealthBar()
    {
        GameObject backgroundObject = new GameObject("PlayerHealthBar", typeof(RectTransform), typeof(Image));
        backgroundObject.transform.SetParent(transform, false);

        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 1f);
        backgroundRect.anchorMax = new Vector2(0f, 1f);
        backgroundRect.pivot = new Vector2(0f, 1f);
        backgroundRect.anchoredPosition = new Vector2(330f, -145f);
        backgroundRect.sizeDelta = new Vector2(300f, 22f);
        backgroundObject.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObject.transform.SetParent(backgroundObject.transform, false);

        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = new Vector2(3f, 3f);
        fillRect.offsetMax = new Vector2(-3f, -3f);

        playerHealthFillRect = fillRect;
        fillObject.GetComponent<Image>().color = new Color(0.2f, 0.85f, 0.3f);
    }
}
