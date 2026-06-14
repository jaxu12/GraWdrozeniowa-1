using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHp = 5;
    [SerializeField] private int currentHp;

    [Header("Enemy Rewards")]
    [SerializeField] private int experienceReward = 1;
    [SerializeField] private bool dropsChestOnDeath;
    [SerializeField] private bool elite;
    [SerializeField] private EnemyRank enemyRank = EnemyRank.Normal;

    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;
    public float NormalizedHp => maxHp <= 0 ? 0f : (float)currentHp / maxHp;
    public bool IsElite => elite;

    private void Start()
    {
        if (currentHp <= 0)
        {
            currentHp = maxHp;
        }

        if (CompareTag("Player"))
        {
            GameManager.Instance?.NotifyPlayerHealthChanged(currentHp, maxHp);
        }
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || currentHp <= 0)
        {
            return;
        }

        currentHp = Mathf.Max(0, currentHp - damage);

        if (CompareTag("Player"))
        {
            GameManager.Instance?.NotifyPlayerHealthChanged(currentHp, maxHp);
        }

        if (currentHp > 0)
        {
            return;
        }

        if (CompareTag("Player"))
        {
            GameManager.Instance?.HandlePlayerDeath();
            return;
        }

        GameManager.Instance?.HandleEnemyDeath(transform.position, experienceReward, dropsChestOnDeath, enemyRank);
        Destroy(gameObject);
    }

    public void SetMaxHp(int newMaxHp, int healAmount = 0)
    {
        maxHp = Mathf.Max(1, newMaxHp);
        currentHp = Mathf.Clamp(currentHp + healAmount, 0, maxHp);

        if (CompareTag("Player"))
        {
            GameManager.Instance?.NotifyPlayerHealthChanged(currentHp, maxHp);
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || currentHp <= 0)
        {
            return;
        }

        currentHp = Mathf.Clamp(currentHp + amount, 0, maxHp);

        if (CompareTag("Player"))
        {
            GameManager.Instance?.NotifyPlayerHealthChanged(currentHp, maxHp);
        }
    }

    public void ConfigureEnemy(int newMaxHp, int xpReward, bool shouldDropChest, EnemyRank rank)
    {
        maxHp = Mathf.Max(1, newMaxHp);
        currentHp = maxHp;
        experienceReward = Mathf.Max(1, xpReward);
        dropsChestOnDeath = shouldDropChest;
        enemyRank = rank;
        elite = rank != EnemyRank.Normal;
    }
}
