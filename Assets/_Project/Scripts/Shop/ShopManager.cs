using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public int damageUpgradeCost = 25;
    public int damageIncrease = 5;

    public void SellAll()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SellAllRunItems();
        }
    }

    public void BuyDamageUpgrade()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.BuyDamageUpgrade(damageUpgradeCost, damageIncrease);
    }
}
