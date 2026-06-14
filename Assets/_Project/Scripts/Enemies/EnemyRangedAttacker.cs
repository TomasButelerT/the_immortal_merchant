using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyRangedAttacker : MonoBehaviour
{
    public EnemyData enemyData;
    public GameObject projectilePrefab;

    private Rigidbody2D body;
    private Transform player;
    private LineRenderer aimLine;
    private Material aimMaterial;
    private float nextAttackTime;
    private bool isAttacking;
    private bool isKnockedBack;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        CreateAimLine();
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
        if (player == null || isAttacking || isKnockedBack || enemyData == null)
        {
            return;
        }

        Vector2 offset = player.position - transform.position;
        float distance = offset.magnitude;

        if (distance <= enemyData.attackRange && Time.time >= nextAttackTime)
        {
            StartCoroutine(ShootRoutine());
            return;
        }

        if (distance < enemyData.preferredDistance * 0.75f)
        {
            body.linearVelocity = -offset.normalized * enemyData.moveSpeed;
        }
        else if (distance > enemyData.preferredDistance)
        {
            body.linearVelocity = offset.normalized * enemyData.moveSpeed;
        }
        else
        {
            body.linearVelocity = Vector2.zero;
        }
    }

    public void ApplyKnockback(Vector2 direction, float force, float duration)
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(KnockbackRoutine(direction.normalized, force, duration));
        }
    }

    private IEnumerator ShootRoutine()
    {
        isAttacking = true;
        body.linearVelocity = Vector2.zero;
        SetAimVisible(true);

        yield return new WaitForSeconds(enemyData.attackWindup);

        if (player != null && projectilePrefab != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            GameObject projectileObject = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            projectileObject.GetComponent<EnemyProjectile>().Initialize(
                direction,
                enemyData.contactDamage,
                enemyData.projectileSpeed);
        }

        SetAimVisible(false);
        yield return new WaitForSeconds(enemyData.attackRecovery);
        nextAttackTime = Time.time + enemyData.damageInterval;
        isAttacking = false;
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float force, float duration)
    {
        isKnockedBack = true;
        body.linearVelocity = direction * force;
        yield return new WaitForSeconds(duration);
        isKnockedBack = false;
    }

    private void CreateAimLine()
    {
        GameObject lineObject = new GameObject("RangedAimLine");
        lineObject.transform.SetParent(transform, false);
        aimMaterial = new Material(Shader.Find("Sprites/Default"));
        aimLine = lineObject.AddComponent<LineRenderer>();
        aimLine.positionCount = 2;
        aimLine.startWidth = 0.07f;
        aimLine.endWidth = 0.07f;
        aimLine.startColor = new Color(1f, 0.25f, 0.25f, 0.9f);
        aimLine.endColor = aimLine.startColor;
        aimLine.sharedMaterial = aimMaterial;
        aimLine.sortingOrder = 25;
        SetAimVisible(false);
    }

    private void LateUpdate()
    {
        if (aimLine != null && aimLine.enabled && player != null)
        {
            aimLine.SetPosition(0, transform.position);
            aimLine.SetPosition(1, player.position);
        }
    }

    private void SetAimVisible(bool visible)
    {
        if (aimLine != null)
        {
            aimLine.enabled = visible;
        }
    }

    private void OnDestroy()
    {
        if (aimMaterial != null)
        {
            Destroy(aimMaterial);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isAttacking = false;
        isKnockedBack = false;
        SetAimVisible(false);

        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
        }
    }
}
