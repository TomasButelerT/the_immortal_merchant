using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class PrototypeChest : MonoBehaviour
{
    public ItemData[] rewardItems;
    public int amount = 1;

    private bool opened;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (opened || !other.CompareTag("Player") || rewardItems == null || rewardItems.Length == 0)
        {
            return;
        }

        opened = true;
        ItemData reward = rewardItems[Random.Range(0, rewardItems.Length)];
        InventoryManager.Instance?.AddItem(reward, amount);
        GetComponent<SpriteRenderer>().color = new Color(0.3f, 0.3f, 0.3f);
    }
}
