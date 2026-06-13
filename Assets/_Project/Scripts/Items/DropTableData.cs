using System;
using UnityEngine;

[Serializable]
public class DropTableEntry
{
    public GameObject pickupPrefab;
    [Min(0f)] public float weight = 1f;
}

[CreateAssetMenu(fileName = "NewDropTable", menuName = "The Immortal Merchant/Drop Table")]
public class DropTableData : ScriptableObject
{
    [Range(0f, 1f)] public float dropChance = 1f;
    public DropTableEntry[] entries;

    public GameObject RollDrop()
    {
        if (entries == null || entries.Length == 0 || UnityEngine.Random.value > dropChance)
        {
            return null;
        }

        float totalWeight = 0f;
        foreach (DropTableEntry entry in entries)
        {
            if (entry != null && entry.pickupPrefab != null)
            {
                totalWeight += Mathf.Max(0f, entry.weight);
            }
        }

        if (totalWeight <= 0f)
        {
            return null;
        }

        float roll = UnityEngine.Random.value * totalWeight;
        foreach (DropTableEntry entry in entries)
        {
            if (entry == null || entry.pickupPrefab == null)
            {
                continue;
            }

            roll -= Mathf.Max(0f, entry.weight);
            if (roll <= 0f)
            {
                return entry.pickupPrefab;
            }
        }

        return entries[entries.Length - 1].pickupPrefab;
    }
}
