using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class DungeonExitPortal : MonoBehaviour
{
    private bool isUnlocked;
    private SpriteRenderer portalRenderer;

    private void Awake()
    {
        portalRenderer = GetComponent<SpriteRenderer>();
        SetUnlocked(false);
    }

    public void SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;

        if (portalRenderer != null)
        {
            portalRenderer.color = unlocked
                ? new Color(0.2f, 1f, 0.45f, 0.9f)
                : new Color(0.45f, 0.12f, 0.12f, 0.65f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isUnlocked || !other.CompareTag("Player"))
        {
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToShop();
        }
    }
}
