using UnityEngine;

public enum UpgradeStatType
{
    Damage,
    MaxHealth,
    MoveSpeed
}

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "The Immortal Merchant/Upgrade")]
public class UpgradeData : ScriptableObject
{
    public string upgradeId;
    public string displayName;
    public UpgradeStatType statType;
    public int baseCost = 20;
    public int costIncreasePerLevel = 15;
    public float valuePerLevel = 5f;
    [TextArea] public string description;

    public int GetCost(int currentLevel)
    {
        return baseCost + currentLevel * costIncreasePerLevel;
    }
}
