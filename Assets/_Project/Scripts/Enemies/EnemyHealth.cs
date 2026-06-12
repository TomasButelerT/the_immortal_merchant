using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 30;
    public int currentHealth;
    public GameObject itemDropPrefab;

    private SpriteRenderer enemyRenderer;
    private Color normalColor;
    private Coroutine hitFlashRoutine;

    private void Awake()
    {
        currentHealth = maxHealth;
        enemyRenderer = GetComponent<SpriteRenderer>();

        if (enemyRenderer != null)
        {
            normalColor = enemyRenderer.color;
        }
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHealth <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(currentHealth - amount, 0);

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
        if (itemDropPrefab != null)
        {
            Instantiate(itemDropPrefab, transform.position, Quaternion.identity);
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
