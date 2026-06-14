using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public struct SpawnVariant
    {
        public ProceduralEnemyVariant.ShapeType shape;
        public Color color;
        public int hp;
        public float moveSpeed;
        public float scale;
    }

    public GameObject enemyPrefab;
    public Transform player;
    public float waveInterval = 3.6f;
    public float minimumWaveInterval = 1.8f;
    public float waveIntervalRampPerSecond = 0.03f;
    public float spawnMarginFromCamera = 1.5f;
    public float arenaHalfWidth = 48f;
    public float arenaHalfHeight = 48f;
    public int eliteEvery = 7;
    public int minibossEvery = 5;
    public int baseWaveSize = 5;
    public int waveGrowthEvery = 3;
    public float chordSpacing = 1.2f;
    public SpawnVariant[] variants;

    private float timer;
    private float elapsedTime;
    private int spawnCount;
    private int waveIndex;

    private void Start()
    {
        timer = waveInterval;

        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
        }

        if (player != null && enemyPrefab != null)
        {
            SpawnWave();
        }
    }

    private void Update()
    {
        if (player == null || GameManager.Instance != null && GameManager.Instance.IsOverlayOpen)
        {
            return;
        }

        elapsedTime += Time.deltaTime;
        timer -= Time.deltaTime;

        if (timer > 0f)
        {
            return;
        }

        SpawnWave();
        float dynamicWaveInterval = Mathf.Max(minimumWaveInterval, waveInterval - elapsedTime * waveIntervalRampPerSecond);
        timer = dynamicWaveInterval;
    }

    private void SpawnWave()
    {
        if (enemyPrefab == null || player == null)
        {
            return;
        }

        waveIndex++;
        int threatTier = GetThreatTier();
        int waveSize = baseWaveSize + Mathf.Max(0, (waveIndex - 1) / Mathf.Max(1, waveGrowthEvery)) + threatTier;
        int side = Random.Range(0, 4);
        bool isMinibossWave = minibossEvery > 0 && waveIndex % minibossEvery == 0;
        bool spawnedEliteThisWave = false;

        for (int i = 0; i < waveSize; i++)
        {
            Vector2 spawnPosition = GetWaveSpawnPosition(side, i, waveSize);
            EnemyRank rank = EnemyRank.Normal;

            if (isMinibossWave && i == waveSize / 2)
            {
                rank = EnemyRank.Miniboss;
            }
            else if (!spawnedEliteThisWave && eliteEvery > 0 && (spawnCount + 1) % eliteEvery == 0)
            {
                rank = EnemyRank.Elite;
                spawnedEliteThisWave = true;
            }

            SpawnEnemy(spawnPosition, rank, threatTier);
        }

        if (threatTier >= 2 && !isMinibossWave)
        {
            SpawnFlankWave(waveSize / 2 + 1);
        }
    }

    private SpawnVariant GetRandomVariant()
    {
        SpawnVariant[] source = variants;
        if (source == null || source.Length == 0)
        {
            source = GetDefaultVariants();
        }

        return source[Random.Range(0, source.Length)];
    }

    private SpawnVariant[] GetDefaultVariants()
    {
        return new[]
        {
            new SpawnVariant { shape = ProceduralEnemyVariant.ShapeType.Square, color = new Color(0.92f, 0.26f, 0.21f, 1f), hp = 1, moveSpeed = 4.2f, scale = 0.34f },
            new SpawnVariant { shape = ProceduralEnemyVariant.ShapeType.Triangle, color = new Color(1f, 0.72f, 0.16f, 1f), hp = 2, moveSpeed = 3.2f, scale = 0.42f },
            new SpawnVariant { shape = ProceduralEnemyVariant.ShapeType.Diamond, color = new Color(0.22f, 0.73f, 0.85f, 1f), hp = 3, moveSpeed = 2.5f, scale = 0.52f },
            new SpawnVariant { shape = ProceduralEnemyVariant.ShapeType.Circle, color = new Color(0.64f, 0.36f, 0.94f, 1f), hp = 4, moveSpeed = 1.9f, scale = 0.6f }
        };
    }

    private void SpawnEnemy(Vector2 spawnPosition, EnemyRank rank, int threatTier)
    {
        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        spawnedEnemy.tag = "Enemy";
        spawnCount++;

        SpawnVariant variantData = GetRandomVariant();
        variantData.hp += threatTier;
        variantData.moveSpeed += threatTier * 0.18f;

        ProceduralEnemyVariant variant = spawnedEnemy.GetComponent<ProceduralEnemyVariant>();
        if (variant == null)
        {
            variant = spawnedEnemy.AddComponent<ProceduralEnemyVariant>();
        }

        variant.Apply(
            variantData.shape,
            variantData.color,
            variantData.hp,
            variantData.moveSpeed,
            variantData.scale,
            rank
        );
    }

    private void SpawnFlankWave(int waveSize)
    {
        int side = Random.Range(0, 4);
        for (int i = 0; i < waveSize; i++)
        {
            Vector2 spawnPosition = GetWaveSpawnPosition(side, i, waveSize);
            SpawnEnemy(spawnPosition, EnemyRank.Normal, Mathf.Max(0, GetThreatTier() - 1));
        }
    }

    private int GetThreatTier()
    {
        if (elapsedTime >= 90f)
        {
            return 3;
        }

        if (elapsedTime >= 55f)
        {
            return 2;
        }

        if (elapsedTime >= 25f)
        {
            return 1;
        }

        return 0;
    }

    private Vector2 GetWaveSpawnPosition(int side, int index, int waveSize)
    {
        Camera mainCamera = Camera.main;
        float halfHeight = GetCameraHalfHeight(mainCamera) + spawnMarginFromCamera;
        float halfWidth = GetCameraHalfWidth(mainCamera) + spawnMarginFromCamera;
        float centeredOffset = (index - (waveSize - 1) * 0.5f) * chordSpacing;

        switch (side)
        {
            case 0:
                return ClampToArena(new Vector2(player.position.x + centeredOffset, player.position.y + halfHeight));
            case 1:
                return ClampToArena(new Vector2(player.position.x + centeredOffset, player.position.y - halfHeight));
            case 2:
                return ClampToArena(new Vector2(player.position.x - halfWidth, player.position.y + centeredOffset));
            default:
                return ClampToArena(new Vector2(player.position.x + halfWidth, player.position.y + centeredOffset));
        }
    }

    private Vector2 ClampToArena(Vector2 candidate)
    {
        candidate.x = Mathf.Clamp(candidate.x, -arenaHalfWidth, arenaHalfWidth);
        candidate.y = Mathf.Clamp(candidate.y, -arenaHalfHeight, arenaHalfHeight);
        return candidate;
    }

    private float GetCameraHalfHeight(Camera mainCamera)
    {
        return mainCamera == null || !mainCamera.orthographic ? 6f : mainCamera.orthographicSize;
    }

    private float GetCameraHalfWidth(Camera mainCamera)
    {
        return mainCamera == null || !mainCamera.orthographic ? 10f : mainCamera.orthographicSize * mainCamera.aspect;
    }
}
