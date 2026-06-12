using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class DungeonDoor : MonoBehaviour
{
    private SpriteRenderer doorRenderer;
    private Collider2D doorCollider;

    private void Awake()
    {
        doorRenderer = GetComponent<SpriteRenderer>();
        doorCollider = GetComponent<Collider2D>();
    }

    public void SetLocked(bool locked)
    {
        doorCollider.enabled = locked;
        doorRenderer.enabled = locked;
    }
}
