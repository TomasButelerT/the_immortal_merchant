using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class PrototypeChest : MonoBehaviour
{
    public ItemData rewardItem;
    public int amount = 1;

    private bool opened;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (opened || !other.CompareTag("Player") || rewardItem == null)
        {
            return;
        }

        opened = true;
        InventoryManager.Instance?.AddItem(rewardItem, amount);
        GetComponent<SpriteRenderer>().color = new Color(0.3f, 0.3f, 0.3f);
    }
}
