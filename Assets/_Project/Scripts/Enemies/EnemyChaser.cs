using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChaser : MonoBehaviour
{
    public EnemyData enemyData;
    public float moveSpeed = 2f;
    public int contactDamage = 10;
    public float damageInterval = 1f;
    public float attackRange = 1.25f;
    public float attackWindup = 0.45f;
    public float attackRecovery = 0.65f;
    public int attackCount = 1;
    public float followUpWindup = 0.18f;
    public float retreatSpeed;
    public float retreatDuration;
    public bool usesAreaAttack;
    public float areaAttackRadius = 2.4f;
    public float vulnerableDuration = 1.1f;

    private Rigidbody2D body;
    private Transform player;
    private float nextAttackTime;
    private bool isKnockedBack;
    private bool isAttacking;
    private Coroutine attackRoutine;
    private LineRenderer attackTelegraph;
    private Material telegraphMaterial;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();

        if (enemyData != null)
        {
            moveSpeed = enemyData.moveSpeed;
            contactDamage = enemyData.contactDamage;
            damageInterval = enemyData.damageInterval;
            attackRange = enemyData.attackRange;
            attackWindup = enemyData.attackWindup;
            attackRecovery = enemyData.attackRecovery;
            attackCount = enemyData.meleeAttackCount;
            followUpWindup = enemyData.followUpWindup;
            retreatSpeed = enemyData.retreatSpeed;
            retreatDuration = enemyData.retreatDuration;
            usesAreaAttack = enemyData.usesAreaAttack;
            areaAttackRadius = enemyData.areaAttackRadius;
            vulnerableDuration = enemyData.vulnerableDuration;
        }

        CreateAttackTelegraph();
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void FixedUpdate()
    {
        if (player == null)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        if (isKnockedBack || isAttacking)
        {
            return;
        }

        Vector2 offset = player.position - transform.position;
        if (offset.magnitude <= attackRange)
        {
            body.linearVelocity = Vector2.zero;
            if (Time.time >= nextAttackTime)
            {
                attackRoutine = StartCoroutine(AttackRoutine());
            }

            return;
        }

        body.linearVelocity = offset.normalized * moveSpeed;
    }

    public void ApplyKnockback(Vector2 direction, float force, float duration)
    {
        if (gameObject.activeInHierarchy)
        {
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
                attackRoutine = null;
                isAttacking = false;
                SetTelegraphVisible(false);
                SetTelegraphRadius(attackRange);
            }

            StartCoroutine(KnockbackRoutine(direction.normalized, force, duration));
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        SetAnchored(true);

        if (usesAreaAttack)
        {
            yield return StartCoroutine(AreaAttackRoutine());
            SetAnchored(false);
            nextAttackTime = Time.time + damageInterval;
            isAttacking = false;
            attackRoutine = null;
            yield break;
        }

        int strikes = Mathf.Max(1, attackCount);

        for (int strike = 0; strike < strikes; strike++)
        {
            float windup = strike == 0 ? attackWindup : followUpWindup;
            yield return StartCoroutine(StrikeWindup(windup, strike));
            yield return StartCoroutine(PerformStrike());
            SetTelegraphVisible(false);
            SetTelegraphRadius(attackRange);

            if (strike < strikes - 1)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        if (retreatSpeed > 0f && retreatDuration > 0f && player != null)
        {
            SetAnchored(false);
            Vector2 retreatDirection = (body.position - (Vector2)player.position).normalized;
            body.linearVelocity = retreatDirection * retreatSpeed;
            yield return new WaitForSeconds(retreatDuration);
            body.linearVelocity = Vector2.zero;
        }

        SetAnchored(true);
        yield return new WaitForSeconds(attackRecovery);
        SetAnchored(false);

        nextAttackTime = Time.time + damageInterval;
        isAttacking = false;
        attackRoutine = null;
    }

    private IEnumerator AreaAttackRoutine()
    {
        SetTelegraphVisible(true);
        float elapsed = 0f;

        while (elapsed < attackWindup)
        {
            elapsed += Time.deltaTime;
            float progress = attackWindup > 0f ? Mathf.Clamp01(elapsed / attackWindup) : 1f;
            SetTelegraphColor(Color.Lerp(
                new Color(0.75f, 0.2f, 1f, 0.9f),
                new Color(1f, 0.1f, 0.1f, 1f),
                progress));
            SetTelegraphRadius(Mathf.Lerp(areaAttackRadius * 0.35f, areaAttackRadius, progress));
            yield return null;
        }

        if (player != null && Vector2.Distance(transform.position, player.position) <= areaAttackRadius)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage, transform.position);
            }
        }

        CombatFeedback.PlayHit(0.045f, 0.18f, 0.2f);
        yield return StartCoroutine(AreaShockwave());
        yield return StartCoroutine(VulnerableRecovery());
        SetTelegraphVisible(false);
        SetTelegraphRadius(attackRange);
        yield return new WaitForSeconds(attackRecovery);
    }

    private IEnumerator AreaShockwave()
    {
        const float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            SetTelegraphRadius(Mathf.Lerp(areaAttackRadius * 0.15f, areaAttackRadius * 1.15f, progress));
            SetTelegraphColor(new Color(1f, 0.75f, 0.15f, 1f - progress));
            yield return null;
        }
    }

    private IEnumerator VulnerableRecovery()
    {
        float elapsed = 0f;
        SetTelegraphVisible(true);
        SetTelegraphRadius(areaAttackRadius * 0.45f);

        while (elapsed < vulnerableDuration)
        {
            elapsed += Time.deltaTime;
            float pulse = 0.55f + Mathf.Sin(elapsed * 12f) * 0.2f;
            SetTelegraphColor(new Color(0.2f, 1f, 0.35f, pulse));
            yield return null;
        }
    }

    private IEnumerator StrikeWindup(float duration, int strikeIndex)
    {
        SetTelegraphVisible(true);
        float elapsed = 0f;
        Color startColor = strikeIndex == 0
            ? new Color(1f, 0.65f, 0.1f, 0.9f)
            : new Color(1f, 0.9f, 0.2f, 0.95f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            SetTelegraphColor(Color.Lerp(startColor, new Color(1f, 0.1f, 0.1f, 1f), progress));
            SetTelegraphRadius(Mathf.Lerp(attackRange, attackRange * 0.65f, progress));
            yield return null;
        }
    }

    private IEnumerator PerformStrike()
    {
        SetAnchored(false);
        Vector2 attackDirection = player != null
            ? ((Vector2)player.position - body.position).normalized
            : Vector2.zero;
        float lungeSpeed = Mathf.Max(2.5f, moveSpeed * 2f);
        body.linearVelocity = attackDirection * lungeSpeed;
        yield return new WaitForSeconds(0.08f);
        body.linearVelocity = Vector2.zero;

        if (player != null && Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage, transform.position);
            }
        }

        SetAnchored(true);
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float force, float duration)
    {
        SetAnchored(false);
        isKnockedBack = true;
        body.linearVelocity = direction * force;
        yield return new WaitForSeconds(duration);
        isKnockedBack = false;
    }

    private void SetAnchored(bool anchored)
    {
        if (body == null || !body.simulated)
        {
            return;
        }

        body.linearVelocity = Vector2.zero;
        body.bodyType = anchored ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
    }

    private void CreateAttackTelegraph()
    {
        GameObject telegraphObject = new GameObject("AttackTelegraph");
        telegraphObject.transform.SetParent(transform, false);
        Vector3 parentScale = transform.lossyScale;
        telegraphObject.transform.localScale = new Vector3(
            parentScale.x != 0f ? 1f / parentScale.x : 1f,
            parentScale.y != 0f ? 1f / parentScale.y : 1f,
            1f);

        telegraphMaterial = new Material(Shader.Find("Sprites/Default"));
        attackTelegraph = telegraphObject.AddComponent<LineRenderer>();
        attackTelegraph.useWorldSpace = false;
        attackTelegraph.loop = true;
        attackTelegraph.positionCount = 40;
        attackTelegraph.startWidth = 0.08f;
        attackTelegraph.endWidth = 0.08f;
        attackTelegraph.sharedMaterial = telegraphMaterial;
        attackTelegraph.sortingOrder = 25;

        for (int i = 0; i < attackTelegraph.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / attackTelegraph.positionCount;
            attackTelegraph.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * attackRange);
        }

        SetTelegraphVisible(false);
    }

    private void SetTelegraphVisible(bool visible)
    {
        if (attackTelegraph != null)
        {
            attackTelegraph.enabled = visible;
        }
    }

    private void SetTelegraphColor(Color color)
    {
        if (attackTelegraph != null)
        {
            attackTelegraph.startColor = color;
            attackTelegraph.endColor = color;
        }
    }

    private void SetTelegraphRadius(float radius)
    {
        if (attackTelegraph == null)
        {
            return;
        }

        for (int i = 0; i < attackTelegraph.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / attackTelegraph.positionCount;
            attackTelegraph.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
        }
    }

    private void OnDestroy()
    {
        if (telegraphMaterial != null)
        {
            Destroy(telegraphMaterial);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        attackRoutine = null;
        isAttacking = false;
        isKnockedBack = false;
        SetAnchored(false);
        SetTelegraphVisible(false);
        SetTelegraphRadius(attackRange);

        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
        }
    }
}
