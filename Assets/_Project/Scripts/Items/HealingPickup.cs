using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class HealingPickup : MonoBehaviour
{
    public int healAmount = 25;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null && health.currentHealth < health.maxHealth)
        {
            health.Heal(healAmount);
            Destroy(gameObject);
        }
    }
}
