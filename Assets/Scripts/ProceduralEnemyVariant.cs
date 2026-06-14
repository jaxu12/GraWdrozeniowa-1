using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ProceduralEnemyVariant : MonoBehaviour
{
    public enum ShapeType
    {
        Square,
        Triangle,
        Diamond,
        Circle
    }

    private static readonly Dictionary<ShapeType, Sprite> SpriteCache = new Dictionary<ShapeType, Sprite>();
    private static Sprite eliteRingSprite;

    public void Apply(ShapeType shape, Color color, int hp, float moveSpeed, float scale, EnemyRank rank)
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = false;
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = GetShapeSprite(shape);
            spriteRenderer.color = GetRankTint(color, rank);
            spriteRenderer.sortingOrder = 12;
        }

        EnemyFollow enemyFollow = GetComponent<EnemyFollow>();
        if (enemyFollow != null)
        {
            enemyFollow.speed = GetMoveSpeed(moveSpeed, rank);
        }

        Health health = GetComponent<Health>();
        if (health != null)
        {
            int finalHp = GetHp(hp, rank);
            int xpReward = GetXpReward(hp, rank);
            bool shouldDropChest = rank == EnemyRank.Miniboss;
            health.ConfigureEnemy(finalHp, xpReward, shouldDropChest, rank);
        }

        float finalScale = GetScale(scale, rank);
        transform.localScale = new Vector3(finalScale, finalScale, 1f);
        UpdateRankVisual(rank);
        gameObject.name = rank + " " + shape + " Enemy";
    }

    private void UpdateRankVisual(EnemyRank rank)
    {
        Transform ring = transform.Find("EliteRing");
        if (rank == EnemyRank.Normal)
        {
            if (ring != null)
            {
                ring.gameObject.SetActive(false);
            }

            return;
        }

        if (ring == null)
        {
            GameObject ringObject = new GameObject("EliteRing");
            ringObject.transform.SetParent(transform, false);
            SpriteRenderer renderer = ringObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetEliteRingSprite();
            renderer.color = new Color(1f, 0.95f, 0.4f, 0.7f);
            renderer.sortingOrder = 11;
            ring = ringObject.transform;
        }

        ring.gameObject.SetActive(true);
        ring.localPosition = Vector3.zero;
        ring.localScale = rank == EnemyRank.Miniboss
            ? new Vector3(2.15f, 2.15f, 1f)
            : new Vector3(1.7f, 1.7f, 1f);
    }

    private static Color GetRankTint(Color baseColor, EnemyRank rank)
    {
        switch (rank)
        {
            case EnemyRank.Miniboss:
                return Color.Lerp(baseColor, new Color(1f, 0.93f, 0.55f, 1f), 0.55f);
            case EnemyRank.Elite:
                return Color.Lerp(baseColor, Color.white, 0.35f);
            default:
                return baseColor;
        }
    }

    private static float GetMoveSpeed(float baseSpeed, EnemyRank rank)
    {
        switch (rank)
        {
            case EnemyRank.Miniboss:
                return baseSpeed * 0.82f;
            case EnemyRank.Elite:
                return baseSpeed * 0.92f;
            default:
                return baseSpeed;
        }
    }

    private static int GetHp(int baseHp, EnemyRank rank)
    {
        switch (rank)
        {
            case EnemyRank.Miniboss:
                return baseHp * 10;
            case EnemyRank.Elite:
                return baseHp * 3;
            default:
                return baseHp;
        }
    }

    private static int GetXpReward(int baseHp, EnemyRank rank)
    {
        switch (rank)
        {
            case EnemyRank.Miniboss:
                return baseHp * 8;
            case EnemyRank.Elite:
                return baseHp * 2;
            default:
                return baseHp;
        }
    }

    private static float GetScale(float baseScale, EnemyRank rank)
    {
        switch (rank)
        {
            case EnemyRank.Miniboss:
                return baseScale * 1.9f;
            case EnemyRank.Elite:
                return baseScale * 1.35f;
            default:
                return baseScale;
        }
    }

    private static Sprite GetShapeSprite(ShapeType shape)
    {
        if (SpriteCache.TryGetValue(shape, out Sprite cachedSprite))
        {
            return cachedSprite;
        }

        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool fill;
                switch (shape)
                {
                    case ShapeType.Square:
                        fill = IsInsideSquare(x, y, size);
                        break;
                    case ShapeType.Triangle:
                        fill = IsInsideTriangle(x, y, size);
                        break;
                    case ShapeType.Diamond:
                        fill = IsInsideDiamond(x, y, size);
                        break;
                    default:
                        fill = IsInsideCircle(x, y, size);
                        break;
                }

                pixels[y * size + x] = fill ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 32f);
        SpriteCache[shape] = sprite;
        return sprite;
    }

    private static Sprite GetEliteRingSprite()
    {
        if (eliteRingSprite != null)
        {
            return eliteRingSprite;
        }

        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        Color32[] pixels = new Color32[size * size];
        float center = (size - 1) * 0.5f;
        float outer = size * 0.38f;
        float inner = size * 0.28f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = dx * dx + dy * dy;
                bool ring = dist <= outer * outer && dist >= inner * inner;
                pixels[y * size + x] = ring ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        eliteRingSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        return eliteRingSprite;
    }

    private static bool IsInsideSquare(int x, int y, int size)
    {
        return x >= 10 && x <= size - 11 && y >= 10 && y <= size - 11;
    }

    private static bool IsInsideTriangle(int x, int y, int size)
    {
        Vector2 a = new Vector2(size * 0.5f, size - 8f);
        Vector2 b = new Vector2(8f, 8f);
        Vector2 c = new Vector2(size - 8f, 8f);
        return IsInsideTriangleArea(new Vector2(x, y), a, b, c);
    }

    private static bool IsInsideDiamond(int x, int y, int size)
    {
        float center = (size - 1) * 0.5f;
        float distance = Mathf.Abs(x - center) + Mathf.Abs(y - center);
        return distance <= size * 0.36f;
    }

    private static bool IsInsideCircle(int x, int y, int size)
    {
        float center = (size - 1) * 0.5f;
        float radius = size * 0.34f;
        float dx = x - center;
        float dy = y - center;
        return dx * dx + dy * dy <= radius * radius;
    }

    private static bool IsInsideTriangleArea(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float denominator = (b.y - c.y) * (a.x - c.x) + (c.x - b.x) * (a.y - c.y);
        float alpha = ((b.y - c.y) * (p.x - c.x) + (c.x - b.x) * (p.y - c.y)) / denominator;
        float beta = ((c.y - a.y) * (p.x - c.x) + (a.x - c.x) * (p.y - c.y)) / denominator;
        float gamma = 1f - alpha - beta;
        return alpha >= 0f && beta >= 0f && gamma >= 0f;
    }
}
