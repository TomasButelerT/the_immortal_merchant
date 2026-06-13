using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class PrototypeChest : MonoBehaviour
{
    public DropTableData rewardTable;
    public ItemData[] rewardItems;
    public int amount = 1;

    private bool opened;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (opened || !other.CompareTag("Player"))
        {
            return;
        }

        if (rewardTable != null)
        {
            GameObject pickupPrefab = rewardTable.RollDrop();
            ItemPickup pickup = pickupPrefab != null ? pickupPrefab.GetComponent<ItemPickup>() : null;
            if (pickup == null || pickup.itemData == null)
            {
                return;
            }

            InventoryManager.Instance?.AddItem(pickup.itemData, amount);
        }
        else
        {
            if (rewardItems == null || rewardItems.Length == 0)
            {
                return;
            }

            ItemData reward = rewardItems[Random.Range(0, rewardItems.Length)];
            InventoryManager.Instance?.AddItem(reward, amount);
        }

        opened = true;
        GetComponent<SpriteRenderer>().color = new Color(0.3f, 0.3f, 0.3f);
    }
}
