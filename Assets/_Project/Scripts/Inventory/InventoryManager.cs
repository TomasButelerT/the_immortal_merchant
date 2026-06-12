using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RunInventoryEntry
{
    public string itemId;
    public ItemData itemData;
    public int amount;

    public RunInventoryEntry(ItemData item, int initialAmount)
    {
        itemId = item.itemId;
        itemData = item;
        amount = initialAmount;
    }
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private List<RunInventoryEntry> runInventory = new List<RunInventoryEntry>();
    public IReadOnlyList<RunInventoryEntry> RunInventory => runInventory;

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

    public void AddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
        {
            return;
        }

        RunInventoryEntry existingEntry = runInventory.Find(entry => entry.itemId == item.itemId);
        if (existingEntry != null)
        {
            existingEntry.amount += amount;
            return;
        }

        runInventory.Add(new RunInventoryEntry(item, amount));
    }

    public void ClearRunInventory()
    {
        runInventory.Clear();
    }

    public void SellAllRunItems()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("A GameManager is required to sell items.");
            return;
        }

        int totalGold = 0;
        foreach (RunInventoryEntry entry in runInventory)
        {
            if (entry.itemData != null)
            {
                totalGold += entry.itemData.sellPrice * entry.amount;
            }
        }

        GameManager.Instance.AddGold(totalGold);
        ClearRunInventory();
    }
}
