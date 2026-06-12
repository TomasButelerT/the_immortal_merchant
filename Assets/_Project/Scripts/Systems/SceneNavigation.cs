using TMPro;
using UnityEngine;

public class SceneNavigation : MonoBehaviour
{
    public void EnterDungeon()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnterDungeon();
        }
    }

    public void ReturnToShop()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToShop();
        }
    }
}

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
