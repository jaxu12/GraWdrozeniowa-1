using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public int damage = 1;
    public float lifeTime = 3f;

    private Vector2 direction;
    private Rigidbody2D rb;
    private Collider2D myCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
        
        rb.gravityScale = 0f; 
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Wymuszenie warstwy pocisków
        gameObject.layer = LayerMask.NameToLayer("Pociski");
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        rb.linearVelocity = direction * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignorowanie gracza
        if (collision.gameObject.CompareTag("Player"))
        {
            Physics2D.IgnoreCollision(myCollider, collision.collider);
            return;
        }

        // Trafienie wroga
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Health enemyHealth = collision.gameObject.GetComponent<Health>();
            if (enemyHealth != null)
            {
                // Zadajemy obrażenia Twoją funkcją
                enemyHealth.TakeDamage(damage);

                // SPRAWDZAMY TWÓJ WARUNEK ŚMIERCI:
                // Sprawdzamy czy Twoje 'hp' spadło do zera LUB czy potwór został wygaszony (SetActive(false))
                if (enemyHealth.hp <= 0 || !collision.gameObject.activeSelf)
                {
                    Destroy(gameObject); // Strzała natychmiast znika, bo potwór nie żyje
                    return; 
                }
            }

            // JEŚLI POTWÓR PRZEŻYŁ:
            // Strzała wyłącza collider i leci ranić kolejnych wrogów w hordzie
            if (myCollider != null)
            {
                myCollider.enabled = false;
            }
        }
    }
}