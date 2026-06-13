using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public int damage = 1;
    public float lifeTime = 3f;

    private Rigidbody2D rb;
    private Vector2 moveDirection = Vector2.right; // Domyślny kierunek

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Nadajemy prędkość w kierunku, który przekazał PlayerMovement
        if (rb != null)
        {
            rb.linearVelocity = moveDirection * speed;
        }
        
        Destroy(gameObject, lifeTime);
    }

    // Ta funkcja naprawi błąd w PlayerMovement.cs!
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;

        // Jeśli strzała ma lecieć w lewo, obracamy jej sprite, żeby nie leciała tyłem
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
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
        // Trafiamy TYLKO potwory
        if (hitObject.CompareTag("Enemy"))
        {
            Health enemyHealth = hitObject.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage); 
            }

            Destroy(gameObject); // Strzała znika po trafieniu wroga
        }
    }
}