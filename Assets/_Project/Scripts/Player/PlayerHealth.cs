using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public float invulnerabilityDuration = 0.75f;

    private bool isInvulnerable;
    private SpriteRenderer playerRenderer;
    private Color normalColor;

    private void Awake()
    {
        currentHealth = maxHealth;
        playerRenderer = GetComponent<SpriteRenderer>();

        if (playerRenderer != null)
        {
            normalColor = playerRenderer.color;
        }
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHealth <= 0 || isInvulnerable)
        {
            return;
        }

        currentHealth = Mathf.Max(currentHealth - amount, 0);

        if (currentHealth == 0)
        {
            Die();
            return;
        }

        StartCoroutine(InvulnerabilityRoutine());
    }

    public void Heal(int amount)
    {
        if (amount > 0 && currentHealth > 0)
        {
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        }
    }

    public void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDeath();
        }

        gameObject.SetActive(false);
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;
        float endTime = Time.time + invulnerabilityDuration;

        while (Time.time < endTime)
        {
            if (playerRenderer != null)
            {
                playerRenderer.color = new Color(normalColor.r, normalColor.g, normalColor.b, 0.35f);
            }

            yield return new WaitForSeconds(0.08f);

            if (playerRenderer != null)
            {
                playerRenderer.color = normalColor;
            }

            yield return new WaitForSeconds(0.08f);
        }

        if (playerRenderer != null)
        {
            playerRenderer.color = normalColor;
        }

        isInvulnerable = false;
    }
}
