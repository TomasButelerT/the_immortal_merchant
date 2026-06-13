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
        LoadProgress();
    }

    public void AddGold(int amount)
    {
        if (amount > 0)
        {
            gold += amount;
            SaveProgress();
        }
    }

    public bool SpendGold(int amount)
    {
        if (amount < 0 || gold < amount)
        {
            return false;
        }

        gold -= amount;
        SaveProgress();
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
        SaveProgress();
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

        SaveProgress();
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
            SaveProgress();
        }

        SceneManager.LoadScene("ShopScene");
    }

    public void OnPlayerDeath()
    {
        RecordRunSummary(false);
        runInProgress = false;
        SaveProgress();

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

    public void SaveProgress()
    {
        SaveSystem.Save(new PermanentProgressData
        {
            gold = gold,
            corruptionLevel = corruptionLevel,
            damageUpgradeLevel = damageUpgradeLevel,
            damageBonus = damageBonus,
            healthUpgradeLevel = healthUpgradeLevel,
            maxHealthBonus = maxHealthBonus,
            moveSpeedUpgradeLevel = moveSpeedUpgradeLevel,
            moveSpeedBonus = moveSpeedBonus,
            lastRunSummary = lastRunSummary
        });
    }

    public void ResetProgress()
    {
        gold = 0;
        corruptionLevel = 0;
        damageUpgradeLevel = 0;
        damageBonus = 0;
        healthUpgradeLevel = 0;
        maxHealthBonus = 0;
        moveSpeedUpgradeLevel = 0;
        moveSpeedBonus = 0f;
        lastRunSummary = "No completed runs yet.";

        InventoryManager.Instance?.ClearRunInventory();
        SaveSystem.Delete();
    }

    private void LoadProgress()
    {
        PermanentProgressData data = SaveSystem.Load();
        if (data == null)
        {
            return;
        }

        gold = data.gold;
        corruptionLevel = data.corruptionLevel;
        damageUpgradeLevel = data.damageUpgradeLevel;
        damageBonus = data.damageBonus;
        healthUpgradeLevel = data.healthUpgradeLevel;
        maxHealthBonus = data.maxHealthBonus;
        moveSpeedUpgradeLevel = data.moveSpeedUpgradeLevel;
        moveSpeedBonus = data.moveSpeedBonus;
        lastRunSummary = string.IsNullOrEmpty(data.lastRunSummary)
            ? "No completed runs yet."
            : data.lastRunSummary;
    }

    private void OnApplicationQuit()
    {
        SaveProgress();
    }
}
