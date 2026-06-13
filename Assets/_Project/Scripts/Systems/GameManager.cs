using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int gold;
    public int corruptionLevel;
    public int damageUpgradeLevel;
    public int damageBonus;
    public int healthUpgradeLevel;
    public int maxHealthBonus;
    public int moveSpeedUpgradeLevel;
    public float moveSpeedBonus;
    public string lastRunSummary = "No completed runs yet.";

    private bool runInProgress;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddGold(int amount)
    {
        if (amount > 0)
        {
            gold += amount;
        }
    }

    public bool SpendGold(int amount)
    {
        if (amount < 0 || gold < amount)
        {
            return false;
        }

        gold -= amount;
        return true;
    }

    public bool BuyDamageUpgrade(int cost, int damageIncrease)
    {
        if (damageIncrease <= 0 || !SpendGold(cost))
        {
            return false;
        }

        damageUpgradeLevel++;
        damageBonus += damageIncrease;
        return true;
    }

    public int GetUpgradeLevel(UpgradeStatType statType)
    {
        switch (statType)
        {
            case UpgradeStatType.Damage:
                return damageUpgradeLevel;
            case UpgradeStatType.MaxHealth:
                return healthUpgradeLevel;
            case UpgradeStatType.MoveSpeed:
                return moveSpeedUpgradeLevel;
            default:
                return 0;
        }
    }

    public bool BuyUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null)
        {
            return false;
        }

        int currentLevel = GetUpgradeLevel(upgrade.statType);
        if (!SpendGold(upgrade.GetCost(currentLevel)))
        {
            return false;
        }

        switch (upgrade.statType)
        {
            case UpgradeStatType.Damage:
                damageUpgradeLevel++;
                damageBonus += Mathf.RoundToInt(upgrade.valuePerLevel);
                break;
            case UpgradeStatType.MaxHealth:
                healthUpgradeLevel++;
                maxHealthBonus += Mathf.RoundToInt(upgrade.valuePerLevel);
                break;
            case UpgradeStatType.MoveSpeed:
                moveSpeedUpgradeLevel++;
                moveSpeedBonus += upgrade.valuePerLevel;
                break;
        }

        return true;
    }

    public void EnterDungeon()
    {
        runInProgress = true;
        SceneManager.LoadScene("DungeonScene");
    }

    public void ReturnToShop()
    {
        if (runInProgress)
        {
            RecordRunSummary(true);
            runInProgress = false;
        }

        SceneManager.LoadScene("ShopScene");
    }

    public void OnPlayerDeath()
    {
        RecordRunSummary(false);
        runInProgress = false;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ClearRunInventory();
        }

        SceneManager.LoadScene("ShopScene");
    }

    private void RecordRunSummary(bool survived)
    {
        int itemCount = InventoryManager.Instance != null
            ? InventoryManager.Instance.GetRunItemCount()
            : 0;
        int inventoryValue = InventoryManager.Instance != null
            ? InventoryManager.Instance.GetRunInventoryValue()
            : 0;

        lastRunSummary = survived
            ? $"Last run: escaped with {itemCount} items worth {inventoryValue} gold."
            : $"Last run: defeated and lost {itemCount} items worth {inventoryValue} gold.";
    }
}
