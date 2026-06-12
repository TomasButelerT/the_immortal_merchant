using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public int damageUpgradeCost = 25;
    public int damageIncrease = 5;
    public int damageUpgradeLevel;

    public void SellAll()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SellAllRunItems();
        }
    }

    public void BuyDamageUpgrade()
    {
        if (GameManager.Instance == null || !GameManager.Instance.SpendGold(damageUpgradeCost))
        {
            return;
        }

        PlayerAttack playerAttack = FindAnyObjectByType<PlayerAttack>();
        if (playerAttack == null)
        {
            GameManager.Instance.AddGold(damageUpgradeCost);
            Debug.LogWarning("No PlayerAttack was found. The purchase was refunded.");
            return;
        }

        playerAttack.damage += damageIncrease;
        damageUpgradeLevel++;
    }
}
