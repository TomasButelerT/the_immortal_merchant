using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RoomTrigger : MonoBehaviour
{
    public RoomEncounter roomEncounter;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && roomEncounter != null)
        {
            roomEncounter.BeginEncounter();
        }
    }
}
