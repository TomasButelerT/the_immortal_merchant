using TMPro;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public TMP_Text enemiesRemainingText;
    public DungeonExitPortal exitPortal;

    private int enemiesRemaining;

    private void Start()
    {
        enemiesRemaining = FindObjectsByType<EnemyHealth>().Length;
        RefreshState();
    }

    public void EnemyDefeated()
    {
        enemiesRemaining = Mathf.Max(enemiesRemaining - 1, 0);
        RefreshState();
    }

    private void RefreshState()
    {
        bool dungeonCleared = enemiesRemaining == 0;

        if (enemiesRemainingText != null)
        {
            enemiesRemainingText.text = dungeonCleared
                ? "Dungeon cleared - Exit portal open"
                : $"Enemies remaining: {enemiesRemaining}";
        }

        if (exitPortal != null)
        {
            exitPortal.SetUnlocked(dungeonCleared);
        }
    }
}
