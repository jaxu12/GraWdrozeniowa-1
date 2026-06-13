using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public int damage = 1;
    public float attackCooldown = 1.2f; 
    public float attackRange = 1.5f;   

    private Transform playerTransform;
    private Health playerHealth;
    private float nextAttackTime = 0f;

    private void Update()
    {
        // Sprawdzamy, czy ten konkretny potwór w ogóle jeszcze istnieje na scenie
        // Jeśli gra go niszczy (Destroy), ten skrypt sam się wyłączy
        if (gameObject == null) return;

        // Szukanie gracza na bieżąco
        if (playerTransform == null || playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerHealth = player.GetComponent<Health>();
            }
            return; 
        }

        // Liczenie dystansu do gracza
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRange)
        {
            if (Time.time >= nextAttackTime)
            {
                // Zadaj obrażenia tylko jeśli gracz i potwór fizycznie żyją
                if (playerHealth != null && playerHealth.hp > 0)
                {
                    playerHealth.TakeDamage(damage);
                    nextAttackTime = Time.time + attackCooldown;
                }
            }
        }
    }

    // Specjalna funkcja Unity wywoływana w ułamku sekundy, gdy pocisk niszczy potwora
    private void OnDestroy()
    {
        // Odcinamy możliwość jakiegokolwiek ataku w klatce śmierci
        playerTransform = null;
        playerHealth = null;
    }
}