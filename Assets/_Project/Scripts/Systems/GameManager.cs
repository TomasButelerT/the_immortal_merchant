using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int gold;
    public int corruptionLevel;
    public int damageUpgradeLevel;
    public int damageBonus;
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
