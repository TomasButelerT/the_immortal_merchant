using TMPro;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class DungeonExitPortal : MonoBehaviour
{
    private bool isUnlocked;
    private SpriteRenderer portalRenderer;
    private TMP_Text statusLabel;

    private void Awake()
    {
        portalRenderer = GetComponent<SpriteRenderer>();
        statusLabel = GetComponentInChildren<TMP_Text>();
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

        if (statusLabel != null)
        {
            statusLabel.text = unlocked ? "EXIT OPEN" : "EXIT LOCKED";
            statusLabel.color = unlocked ? Color.green : new Color(1f, 0.35f, 0.35f);
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
