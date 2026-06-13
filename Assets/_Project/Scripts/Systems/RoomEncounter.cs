using System.Collections.Generic;
using UnityEngine;

public class RoomEncounter : MonoBehaviour
{
    public int roomNumber = 1;
    public bool startsActive;
    public bool isFinalRoom;
    public List<EnemyHealth> enemies = new List<EnemyHealth>();
    public List<DungeonDoor> doors = new List<DungeonDoor>();
    public DungeonManager dungeonManager;
    public DungeonExitPortal completionPortal;

    private bool hasStarted;

    private void Start()
    {
        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.gameObject.SetActive(startsActive);
            }
        }

        if (startsActive)
        {
            BeginEncounter();
        }
    }

    public void BeginEncounter()
    {
        if (hasStarted)
        {
            return;
        }

        hasStarted = true;
        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.gameObject.SetActive(true);
            }
        }

        SetDoorsLocked(true);
        int livingEnemies = CountLivingEnemies();
        dungeonManager.RoomStarted(roomNumber, livingEnemies);

        if (livingEnemies == 0)
        {
            CompleteRoom();
        }
    }

    public void EnemyDefeated(EnemyHealth defeatedEnemy)
    {
        enemies.Remove(defeatedEnemy);
        int livingEnemies = CountLivingEnemies();
        dungeonManager.RoomProgress(roomNumber, livingEnemies);

        if (livingEnemies == 0)
        {
            CompleteRoom();
        }
    }

    private void CompleteRoom()
    {
        SetDoorsLocked(false);
        dungeonManager.RoomCleared(roomNumber, isFinalRoom);

        if (completionPortal != null)
        {
            completionPortal.SetUnlocked(true);
        }
    }

    private int CountLivingEnemies()
    {
        enemies.RemoveAll(enemy => enemy == null);
        return enemies.Count;
    }

    private void SetDoorsLocked(bool locked)
    {
        foreach (DungeonDoor door in doors)
        {
            if (door != null)
            {
                door.SetLocked(locked);
            }
        }
    }
}
