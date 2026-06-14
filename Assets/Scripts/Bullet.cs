using UnityEngine;
using System.Collections.Generic;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 18f;
    [SerializeField] private int damage = 2;
    [SerializeField] private float lifeTime = 2f;

    private Rigidbody2D rb;
    private Vector2 moveDirection = Vector2.right;
    private int remainingHits = 1;
    private readonly HashSet<GameObject> hitEnemies = new HashSet<GameObject>();
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (rb != null)
        {
            rb.linearVelocity = moveDirection * speed;
        }

        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        if (rb != null)
        {
            rb.linearVelocity = moveDirection * speed;
        }
    }

    public void Configure(int newDamage, float newScale, float newSpeed, int pierceCount = 0, float newLifetime = -1f, Color? tint = null)
    {
        damage = Mathf.Max(1, newDamage);
        speed = Mathf.Max(1f, newSpeed);
        remainingHits = Mathf.Max(1, pierceCount + 1);
        transform.localScale = new Vector3(newScale, newScale, 1f);

        if (newLifetime > 0f)
        {
            lifeTime = newLifetime;
        }

        if (tint.HasValue)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = tint.Value;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleHit(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject);
    }

    private void HandleHit(GameObject hitObject)
    {
        if (!hitObject.CompareTag("Enemy"))
        {
            return;
        }

        Health enemyHealth = hitObject.GetComponent<Health>();
        if (enemyHealth != null)
        {
            if (hitEnemies.Contains(hitObject))
            {
                return;
            }

            hitEnemies.Add(hitObject);
            enemyHealth.TakeDamage(damage);
        }

        remainingHits--;
        if (remainingHits <= 0)
        {
            Destroy(gameObject);
        }
    }
}
