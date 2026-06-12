using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int gold;
    public int corruptionLevel;
    public int damageUpgradeLevel;
    public int damageBonus;

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
        SceneManager.LoadScene("DungeonScene");
    }

    public void ReturnToShop()
    {
        SceneManager.LoadScene("ShopScene");
    }

    public void OnPlayerDeath()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ClearRunInventory();
        }

        ReturnToShop();
    }
}
