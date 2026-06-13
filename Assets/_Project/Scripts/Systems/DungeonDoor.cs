using TMPro;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class DungeonDoor : MonoBehaviour
{
    private SpriteRenderer doorRenderer;
    private Collider2D doorCollider;
    private TMP_Text statusLabel;

    private void Awake()
    {
        doorRenderer = GetComponent<SpriteRenderer>();
        doorCollider = GetComponent<Collider2D>();
        statusLabel = GetComponentInChildren<TMP_Text>();
    }

    public void SetLocked(bool locked)
    {
        doorCollider.enabled = locked;
        doorRenderer.enabled = locked;

        if (statusLabel != null)
        {
            statusLabel.gameObject.SetActive(locked);
            statusLabel.text = "DOOR LOCKED";
        }
    }
}
