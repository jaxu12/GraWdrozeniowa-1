using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform player;
    public float spawnDistance = 4f; // Zmniejszone na 4, żeby było je widać!
    public float spawnRate = 2f;

    private float timer;

    private void Start()
    {
        timer = spawnRate;

        // AUTOMATYCZNE RATOWANIE: Jeśli zapomnisz przypisać gracza w Inspectorze,
        // skrypt sam znajdzie go na scenie po uruchomieniu gry!
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
        }
    }

    private void Update()
    {
        if (player == null) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnEnemy();
            timer = spawnRate;
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null || player == null) return;

        Vector2 dir = Random.insideUnitCircle.normalized;
        Vector2 pos = (Vector2)player.position + dir * spawnDistance;

        // Tworzymy potwora i zapisujemy go do zmiennej 'spawnedEnemy'
        GameObject spawnedEnemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
        
        // --- WYMUSZENIE TAGU W LOCIE ---
        // Ta linijka naprawi problem, nawet jeśli zapomnisz ustawić tag w edytorze!
        spawnedEnemy.tag = "Enemy"; 
    }
}