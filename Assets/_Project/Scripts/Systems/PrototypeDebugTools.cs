using UnityEngine;

public class PrototypeDebugTools : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public GameObject debugPanel;

    private void Awake()
    {
        if (debugPanel != null)
        {
            debugPanel.SetActive(Debug.isDebugBuild || Application.isEditor);
        }
    }

    public void AddTestGold()
    {
        GameManager.Instance?.AddGold(100);
    }

    public void HealPlayer()
    {
        if (playerHealth != null)
        {
            playerHealth.Heal(playerHealth.maxHealth);
        }
    }

    public void ClearActiveRoom()
    {
        RoomEncounter[] rooms = FindObjectsByType<RoomEncounter>();
        foreach (RoomEncounter room in rooms)
        {
            if (!room.HasStarted || room.IsCleared)
            {
                continue;
            }

            room.DebugClearRoom();
            return;
        }
    }

    public void LogSavePath()
    {
        Debug.Log($"Prototype save path: {SaveSystem.SavePath}");
    }
}
