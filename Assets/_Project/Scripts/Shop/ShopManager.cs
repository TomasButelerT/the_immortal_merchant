using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public UpgradeData damageUpgrade;
    public UpgradeData healthUpgrade;
    public UpgradeData moveSpeedUpgrade;

    public int damageUpgradeCost = 25;
    public int damageUpgradeCostIncrease = 15;
    public int damageIncrease = 5;

    public int CurrentDamageUpgradeCost => damageUpgradeCost
        + (GameManager.Instance != null ? GameManager.Instance.damageUpgradeLevel * damageUpgradeCostIncrease : 0);

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

        if (damageUpgrade != null)
        {
            GameManager.Instance.BuyUpgrade(damageUpgrade);
        }
        else
        {
            GameManager.Instance.BuyDamageUpgrade(CurrentDamageUpgradeCost, damageIncrease);
        }
    }

    public void BuyHealthUpgrade()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.BuyUpgrade(healthUpgrade);
        }
    }

    public void BuyMoveSpeedUpgrade()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.BuyUpgrade(moveSpeedUpgrade);
        }
    }

    public int GetUpgradeCost(UpgradeData upgrade)
    {
        if (upgrade == null || GameManager.Instance == null)
        {
            return 0;
        }

        return upgrade.GetCost(GameManager.Instance.GetUpgradeLevel(upgrade.statType));
    }

    public void ResetSave()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetProgress();
        }
    }
}
