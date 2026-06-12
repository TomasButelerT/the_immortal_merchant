using System.Text;
using TMPro;
using UnityEngine;

public class SimpleUIManager : MonoBehaviour
{
    public TMP_Text goldText;
    public TMP_Text healthText;
    public TMP_Text inventoryText;
    public TMP_Text upgradeText;
    public PlayerHealth playerHealth;

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
                builder.Append($"\n{itemName} x{entry.amount}");
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
}
