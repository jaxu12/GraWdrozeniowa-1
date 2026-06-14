using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public int damage = 1;
    public float attackCooldown = 1.2f;
    public float attackRange = 1.5f;

    private Transform playerTransform;
    private Health playerHealth;
    private float nextAttackTime;

    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        if (playerTransform == null || playerHealth == null)
        {
            FindPlayer();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > attackRange)
        {
            return;
        }

        if (Time.time >= nextAttackTime && playerHealth.CurrentHp > 0)
        {
            playerHealth.TakeDamage(damage);
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            return;
        }

        playerTransform = player.transform;
        playerHealth = player.GetComponent<Health>();
    }
}
