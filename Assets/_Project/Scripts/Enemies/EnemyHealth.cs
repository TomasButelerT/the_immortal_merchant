using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public EnemyData enemyData;
    public int maxHealth = 30;
    public int currentHealth;
    public GameObject itemDropPrefab;
    [Range(0f, 1f)] public float itemDropChance = 1f;

    private SpriteRenderer enemyRenderer;
    private Color normalColor;
    private Coroutine hitFlashRoutine;
    private EnemyHealthBar healthBar;

    private void Awake()
    {
        if (enemyData != null)
        {
            maxHealth = enemyData.maxHealth;
        }

        currentHealth = maxHealth;
        enemyRenderer = GetComponent<SpriteRenderer>();

        if (enemyRenderer != null)
        {
            normalColor = enemyRenderer.color;
        }

        healthBar = GetComponent<EnemyHealthBar>();
        if (healthBar == null)
        {
            healthBar = gameObject.AddComponent<EnemyHealthBar>();
        }
    }

    public void TakeDamage(int amount)
    {
        TakeDamage(amount, Vector2.zero, 0f, 0f);
    }

    public void TakeDamage(int amount, Vector2 knockbackDirection, float knockbackForce, float knockbackDuration)
    {
        if (amount <= 0 || currentHealth <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(currentHealth - amount, 0);
        healthBar.SetHealth(currentHealth, maxHealth);

        EnemyChaser chaser = GetComponent<EnemyChaser>();
        if (chaser != null && knockbackForce > 0f)
        {
            chaser.ApplyKnockback(knockbackDirection, knockbackForce, knockbackDuration);
        }

        if (hitFlashRoutine != null)
        {
            StopCoroutine(hitFlashRoutine);
        }

        hitFlashRoutine = StartCoroutine(HitFlash());

        if (currentHealth == 0)
        {
            Die();
        }
    }

    public void Die()
    {
        RoomEncounter roomEncounter = GetComponentInParent<RoomEncounter>();
        if (roomEncounter != null)
        {
            roomEncounter.EnemyDefeated(this);
        }

        GameObject dropPrefab = enemyData != null && enemyData.dropTable != null
            ? enemyData.dropTable.RollDrop()
            : (itemDropPrefab != null && Random.value <= itemDropChance ? itemDropPrefab : null);

        if (dropPrefab != null)
        {
            Instantiate(dropPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private IEnumerator HitFlash()
    {
        if (enemyRenderer == null)
        {
            yield break;
        }

        enemyRenderer.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        enemyRenderer.color = normalColor;
        hitFlashRoutine = null;
    }
}

public class EnemyHealthBar : MonoBehaviour
{
    private const float BarWidth = 0.9f;

    private LineRenderer backgroundLine;
    private LineRenderer fillLine;
    private Material lineMaterial;

    private void Awake()
    {
        lineMaterial = new Material(Shader.Find("Sprites/Default"));
        backgroundLine = CreateLine("HealthBarBackground", new Color(0.12f, 0.12f, 0.12f), 0.14f, 20);
        fillLine = CreateLine("HealthBarFill", new Color(0.9f, 0.12f, 0.12f), 0.09f, 21);
        SetHealth(1, 1);
    }

    private void OnDestroy()
    {
        if (lineMaterial != null)
        {
            Destroy(lineMaterial);
        }
    }

    public void SetHealth(int currentHealth, int maxHealth)
    {
        if (fillLine == null)
        {
            return;
        }

        float normalizedHealth = maxHealth > 0
            ? Mathf.Clamp01((float)currentHealth / maxHealth)
            : 0f;
        float leftEdge = -BarWidth * 0.5f;
        float rightEdge = leftEdge + BarWidth * normalizedHealth;

        fillLine.SetPosition(0, new Vector3(leftEdge, 0.75f, 0f));
        fillLine.SetPosition(1, new Vector3(rightEdge, 0.75f, 0f));
    }

    private LineRenderer CreateLine(string objectName, Color color, float width, int sortingOrder)
    {
        GameObject lineObject = new GameObject(objectName);
        lineObject.transform.SetParent(transform, false);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.positionCount = 2;
        line.startWidth = width;
        line.endWidth = width;
        line.startColor = color;
        line.endColor = color;
        line.sharedMaterial = lineMaterial;
        line.sortingOrder = sortingOrder;
        line.numCapVertices = 2;
        line.SetPosition(0, new Vector3(-BarWidth * 0.5f, 0.75f, 0f));
        line.SetPosition(1, new Vector3(BarWidth * 0.5f, 0.75f, 0f));
        return line;
    }
}
