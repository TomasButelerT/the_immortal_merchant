using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChaser : MonoBehaviour
{
    public float moveSpeed = 2f;
    public int contactDamage = 10;
    public float damageInterval = 1f;

    private Rigidbody2D body;
    private Transform player;
    private float nextDamageTime;
    private bool isKnockedBack;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
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

        if (isKnockedBack)
        {
            return;
        }

        Vector2 direction = (player.position - transform.position).normalized;
        body.linearVelocity = direction * moveSpeed;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || Time.time < nextDamageTime)
        {
            return;
        }

        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage, transform.position);
            nextDamageTime = Time.time + damageInterval;
        }
    }

    public void ApplyKnockback(Vector2 direction, float force, float duration)
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(KnockbackRoutine(direction.normalized, force, duration));
        }
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float force, float duration)
    {
        isKnockedBack = true;
        body.linearVelocity = direction * force;
        yield return new WaitForSeconds(duration);
        isKnockedBack = false;
    }
}
