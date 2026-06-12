using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int gold;
    public int corruptionLevel;

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

    public void OnPlayerDeath()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ClearRunInventory();
        }

        // Later this can return to the shop or restart the dungeon.
    }
}
