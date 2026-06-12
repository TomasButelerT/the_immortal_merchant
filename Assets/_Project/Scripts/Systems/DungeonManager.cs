using TMPro;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public TMP_Text enemiesRemainingText;
    public DungeonExitPortal exitPortal;
    public DungeonExitPortal earlyExitPortal;

    public void RoomStarted(int roomNumber, int enemiesRemaining)
    {
        UpdateRoomStatus(roomNumber, enemiesRemaining, false);
    }

    public void RoomProgress(int roomNumber, int enemiesRemaining)
    {
        UpdateRoomStatus(roomNumber, enemiesRemaining, false);
    }

    public void RoomCleared(int roomNumber, bool isFinalRoom)
    {
        UpdateRoomStatus(roomNumber, 0, true);

        if (roomNumber == 1 && earlyExitPortal != null)
        {
            earlyExitPortal.SetUnlocked(true);
        }

        if (isFinalRoom && exitPortal != null)
        {
            exitPortal.SetUnlocked(true);
        }
    }

    private void UpdateRoomStatus(int roomNumber, int enemiesRemaining, bool cleared)
    {
        if (enemiesRemainingText == null)
        {
            return;
        }

        enemiesRemainingText.text = cleared
            ? $"Room {roomNumber} cleared"
            : $"Room {roomNumber} - Enemies: {enemiesRemaining}";
    }
}
