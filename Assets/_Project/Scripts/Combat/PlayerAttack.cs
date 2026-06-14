using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    public Transform attackPoint;
    public float attackDistance = 0.8f;
    public float attackRadius = 0.75f;
    public int damage = 10;
    public float attackCooldown = 0.4f;
    public float knockbackForce = 7f;
    public float knockbackDuration = 0.15f;
    public float comboResetTime = 0.8f;
    public float firstAttackRecovery = 0.22f;
    public float secondAttackRecovery = 0.28f;
    public float finisherRecovery = 0.5f;
    public float finisherDamageMultiplier = 1.8f;
    public float finisherRadiusMultiplier = 1.2f;
    public float finisherKnockbackMultiplier = 1.6f;
    public float attackLungeSpeed = 2.8f;
    public float attackLungeDuration = 0.07f;
    public float finisherLungeMultiplier = 1.5f;
    public LayerMask enemyLayers;

    private float nextAttackTime;
    private float comboExpireTime;
    private int comboStep;
    private bool isAttacking;
    private bool attackQueued;
    private int activeComboStep;
    private Coroutine attackRoutine;
    private PlayerController2D playerController;
    private SpriteRenderer playerRenderer;
    private Color normalColor;
    private LineRenderer attackAreaRenderer;
    private Material attackAreaMaterial;

    private void Awake()
    {
        playerController = GetComponent<PlayerController2D>();
        playerRenderer = GetComponent<SpriteRenderer>();

        if (playerRenderer != null)
        {
            normalColor = playerRenderer.color;
        }

        CreateAttackAreaRenderer();
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
        if (attackPoint == null)
        {
            return;
        }

        if (isAttacking || Time.time < nextAttackTime)
        {
            attackQueued = true;
            return;
        }

        if (Time.time > comboExpireTime)
        {
            comboStep = 0;
        }

        comboStep = comboStep % 3 + 1;
        attackRoutine = StartCoroutine(AttackRoutine(comboStep));
    }

    private IEnumerator AttackRoutine(int step)
    {
        isAttacking = true;
        activeComboStep = step;
        bool isFinisher = step == 3;
        float currentRadius = isFinisher ? attackRadius * finisherRadiusMultiplier : attackRadius;
        int currentDamage = isFinisher ? Mathf.RoundToInt(damage * finisherDamageMultiplier) : damage;
        float currentKnockback = isFinisher ? knockbackForce * finisherKnockbackMultiplier : knockbackForce;
        float recovery = step == 1
            ? firstAttackRecovery
            : (step == 2 ? secondAttackRecovery : finisherRecovery);

        if (isFinisher && playerController != null)
        {
            playerController.SetMovementLocked(true);
        }

        if (playerController != null)
        {
            float lungeSpeed = isFinisher ? attackLungeSpeed * finisherLungeMultiplier : attackLungeSpeed;
            playerController.ApplyAttackLunge(playerController.FacingDirection, lungeSpeed, attackLungeDuration);
        }

        yield return StartCoroutine(AttackFlash(step, currentRadius));
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, currentRadius, enemyLayers);
        HashSet<EnemyHealth> damagedEnemies = new HashSet<EnemyHealth>();

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemyHealth = hit.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null && damagedEnemies.Add(enemyHealth))
            {
                Vector2 knockbackDirection = enemyHealth.transform.position - transform.position;
                enemyHealth.TakeDamage(currentDamage, knockbackDirection, currentKnockback, knockbackDuration);
            }
        }

        if (damagedEnemies.Count > 0)
        {
            CombatFeedback.PlayHit(
                isFinisher ? 0.065f : 0.035f,
                isFinisher ? 0.16f : 0.1f,
                isFinisher ? 0.22f : 0.1f);
        }

        yield return new WaitForSeconds(recovery);

        if (isFinisher && playerController != null)
        {
            playerController.SetMovementLocked(false);
        }

        comboExpireTime = Time.time + comboResetTime;
        nextAttackTime = Time.time;
        isAttacking = false;
        activeComboStep = 0;
        attackRoutine = null;

        if (isFinisher)
        {
            comboStep = 0;
        }

        if (attackQueued)
        {
            attackQueued = false;
            Attack();
        }
    }

    public bool TryCancelForDash()
    {
        if (!isAttacking)
        {
            return true;
        }

        if (activeComboStep == 3)
        {
            return false;
        }

        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        isAttacking = false;
        activeComboStep = 0;
        attackQueued = false;
        HideAttackArea();

        if (playerController != null)
        {
            playerController.CancelAttackLunge();
        }

        if (playerRenderer != null)
        {
            playerRenderer.color = normalColor;
        }

        return true;
    }

    private void UpdateAttackDirection()
    {
        if (attackPoint != null && playerController != null)
        {
            attackPoint.localPosition = playerController.FacingDirection * attackDistance;
        }
    }

    private IEnumerator AttackFlash(int step, float radius)
    {
        Color attackColor = step == 1
            ? new Color(0.55f, 0.9f, 1f)
            : (step == 2 ? new Color(0.35f, 1f, 0.65f) : new Color(1f, 0.8f, 0.2f));
        float flashDuration = step == 3 ? 0.16f : 0.08f;

        if (playerRenderer != null)
        {
            playerRenderer.color = attackColor;
        }

        ShowAttackArea(radius, attackColor);
        yield return new WaitForSeconds(flashDuration);
        HideAttackArea();

        if (playerRenderer != null)
        {
            playerRenderer.color = normalColor;
        }
    }

    private void CreateAttackAreaRenderer()
    {
        if (attackPoint == null)
        {
            return;
        }

        GameObject areaObject = new GameObject("AttackAreaPreview");
        areaObject.transform.SetParent(attackPoint, false);

        Vector3 parentScale = attackPoint.lossyScale;
        areaObject.transform.localScale = new Vector3(
            parentScale.x != 0f ? 1f / parentScale.x : 1f,
            parentScale.y != 0f ? 1f / parentScale.y : 1f,
            1f);

        attackAreaMaterial = new Material(Shader.Find("Sprites/Default"));
        attackAreaRenderer = areaObject.AddComponent<LineRenderer>();
        attackAreaRenderer.useWorldSpace = false;
        attackAreaRenderer.loop = true;
        attackAreaRenderer.positionCount = 40;
        attackAreaRenderer.startWidth = 0.08f;
        attackAreaRenderer.endWidth = 0.08f;
        attackAreaRenderer.sharedMaterial = attackAreaMaterial;
        attackAreaRenderer.sortingOrder = 30;
        HideAttackArea();
    }

    private void ShowAttackArea(float radius, Color color)
    {
        if (attackAreaRenderer == null)
        {
            return;
        }

        for (int i = 0; i < attackAreaRenderer.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / attackAreaRenderer.positionCount;
            attackAreaRenderer.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
        }

        attackAreaRenderer.startColor = color;
        attackAreaRenderer.endColor = color;
        attackAreaRenderer.enabled = true;
    }

    private void HideAttackArea()
    {
        if (attackAreaRenderer != null)
        {
            attackAreaRenderer.enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (attackAreaMaterial != null)
        {
            Destroy(attackAreaMaterial);
        }
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
