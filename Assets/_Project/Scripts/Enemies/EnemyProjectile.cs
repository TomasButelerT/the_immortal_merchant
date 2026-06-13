using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour
{
    public int damage = 10;
    public float speed = 7f;
    public float lifetime = 4f;

    private Vector2 direction;

    public void Initialize(Vector2 travelDirection, int projectileDamage, float projectileSpeed)
    {
        direction = travelDirection.normalized;
        damage = projectileDamage;
        speed = projectileSpeed;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage, transform.position);
            }

            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger && other.GetComponentInParent<EnemyHealth>() == null)
        {
            Destroy(gameObject);
        }
    }
}
