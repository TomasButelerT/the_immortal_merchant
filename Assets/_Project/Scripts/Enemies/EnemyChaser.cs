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
            }

            StartCoroutine(KnockbackRoutine(direction.normalized, force, duration));
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        body.linearVelocity = Vector2.zero;
        SetTelegraphVisible(true);
        SetTelegraphColor(new Color(1f, 0.65f, 0.1f, 0.9f));

        yield return new WaitForSeconds(attackWindup);

        SetTelegraphColor(new Color(1f, 0.1f, 0.1f, 1f));
        if (player != null && Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage, transform.position);
            }
        }

        yield return new WaitForSeconds(0.1f);
        SetTelegraphVisible(false);
        yield return new WaitForSeconds(attackRecovery);

        nextAttackTime = Time.time + damageInterval;
        isAttacking = false;
        attackRoutine = null;
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float force, float duration)
    {
        isKnockedBack = true;
        body.linearVelocity = direction * force;
        yield return new WaitForSeconds(duration);
        isKnockedBack = false;
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

    private void OnDestroy()
    {
        if (telegraphMaterial != null)
        {
            Destroy(telegraphMaterial);
        }
    }
}
