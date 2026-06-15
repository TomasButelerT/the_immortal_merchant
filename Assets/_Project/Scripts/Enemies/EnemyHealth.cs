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
    private bool isDying;

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

        EnemyContactDamage contactDamage = GetComponent<EnemyContactDamage>();
        if (contactDamage == null)
        {
            contactDamage = gameObject.AddComponent<EnemyContactDamage>();
        }

        int damage = enemyData != null ? enemyData.contactDamage : 10;
        float interval = enemyData != null ? enemyData.damageInterval : 1f;
        contactDamage.Configure(damage, interval);
    }

    public void TakeDamage(int amount)
    {
        TakeDamage(amount, Vector2.zero, 0f, 0f);
    }

    public void TakeDamage(int amount, Vector2 knockbackDirection, float knockbackForce, float knockbackDuration)
    {
        if (amount <= 0 || currentHealth <= 0 || isDying)
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

        EnemyRangedAttacker rangedAttacker = GetComponent<EnemyRangedAttacker>();
        if (rangedAttacker != null && knockbackForce > 0f)
        {
            rangedAttacker.ApplyKnockback(knockbackDirection, knockbackForce, knockbackDuration);
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
        if (isDying)
        {
            return;
        }

        isDying = true;
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

        DisableEnemyBehaviour();
        StartCoroutine(DeathRoutine());
    }

    private void DisableEnemyBehaviour()
    {
        foreach (Collider2D enemyCollider in GetComponentsInChildren<Collider2D>())
        {
            enemyCollider.enabled = false;
        }

        EnemyChaser chaser = GetComponent<EnemyChaser>();
        if (chaser != null)
        {
            chaser.enabled = false;
        }

        EnemyRangedAttacker rangedAttacker = GetComponent<EnemyRangedAttacker>();
        if (rangedAttacker != null)
        {
            rangedAttacker.enabled = false;
        }

        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.simulated = false;
        }
    }

    private IEnumerator DeathRoutine()
    {
        const float duration = 0.22f;
        Vector3 startScale = transform.localScale;
        Color startColor = enemyRenderer != null ? normalColor : Color.white;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(startScale, startScale * 0.15f, progress);
            transform.Rotate(0f, 0f, 540f * Time.unscaledDeltaTime);

            if (enemyRenderer != null)
            {
                enemyRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 1f - progress);
            }

            yield return null;
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

public class EnemyContactDamage : MonoBehaviour
{
    private int damage = 10;
    private float damageInterval = 1f;
    private float nextDamageTime;

    public void Configure(int amount, float interval)
    {
        damage = Mathf.Max(0, amount);
        damageInterval = Mathf.Max(0.1f, interval);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision.collider);
    }

    private void TryDamagePlayer(Collider2D other)
    {
        if (Time.time < nextDamageTime || !other.CompareTag("Player"))
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            return;
        }

        playerHealth.TakeDamage(damage, transform.position);
        nextDamageTime = Time.time + damageInterval;
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
