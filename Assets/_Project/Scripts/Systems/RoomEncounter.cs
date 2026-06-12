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
        dungeonManager.RoomStarted(roomNumber, CountLivingEnemies());
    }

    public void EnemyDefeated(EnemyHealth defeatedEnemy)
    {
        enemies.Remove(defeatedEnemy);
        int livingEnemies = CountLivingEnemies();
        dungeonManager.RoomProgress(roomNumber, livingEnemies);

        if (livingEnemies == 0)
        {
            SetDoorsLocked(false);
            dungeonManager.RoomCleared(roomNumber, isFinalRoom);
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
