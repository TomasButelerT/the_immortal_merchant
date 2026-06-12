using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    public Transform attackPoint;
    public float attackDistance = 0.8f;
    public float attackRadius = 0.75f;
    public int damage = 10;
    public float attackCooldown = 0.4f;
    public LayerMask enemyLayers;

    private float nextAttackTime;
    private PlayerController2D playerController;
    private SpriteRenderer playerRenderer;
    private Color normalColor;

    private void Awake()
    {
        playerController = GetComponent<PlayerController2D>();
        playerRenderer = GetComponent<SpriteRenderer>();

        if (playerRenderer != null)
        {
            normalColor = playerRenderer.color;
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            damage += GameManager.Instance.damageBonus;
        }
    }

    private void Update()
    {
        UpdateAttackDirection();

        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        bool attackPressed = (keyboard != null && keyboard.jKey.wasPressedThisFrame)
            || (mouse != null && mouse.leftButton.wasPressedThisFrame);

        if (attackPressed)
        {
            Attack();
        }
    }

    public void Attack()
    {
        if (attackPoint == null || Time.time < nextAttackTime)
        {
            return;
        }

        nextAttackTime = Time.time + attackCooldown;
        StartCoroutine(AttackFlash());
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayers);

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemyHealth = hit.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
        }
    }

    private void UpdateAttackDirection()
    {
        if (attackPoint != null && playerController != null)
        {
            attackPoint.localPosition = playerController.FacingDirection * attackDistance;
        }
    }

    private IEnumerator AttackFlash()
    {
        if (playerRenderer == null)
        {
            yield break;
        }

        playerRenderer.color = new Color(0.55f, 0.9f, 1f);
        yield return new WaitForSeconds(0.08f);
        playerRenderer.color = normalColor;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
