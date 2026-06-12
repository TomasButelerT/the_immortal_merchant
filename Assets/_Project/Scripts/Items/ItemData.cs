using UnityEngine;

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Cursed
}

[CreateAssetMenu(fileName = "NewItem", menuName = "The Immortal Merchant/Item")]
public class ItemData : ScriptableObject
{
    public string itemId;
    public string displayName;
    public int sellPrice;
    public ItemRarity rarity;
    [TextArea] public string description;
    public Sprite icon;
}
