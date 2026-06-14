using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class StagePickup : MonoBehaviour
{
    public enum PickupType
    {
        Heal,
        Magnet
    }

    private static readonly List<StagePickup> ActivePickups = new List<StagePickup>();
    private static readonly Dictionary<PickupType, Sprite> SpriteCache = new Dictionary<PickupType, Sprite>();

    [SerializeField] private PickupType pickupType;
    [SerializeField] private int healAmount = 2;

    public static int ActiveCount => ActivePickups.Count;

    public static void Create(Vector3 position, PickupType type)
    {
        GameObject pickup = new GameObject(type + "Pickup");
        pickup.transform.position = new Vector3(position.x, position.y, 0f);

        SpriteRenderer renderer = pickup.AddComponent<SpriteRenderer>();
        renderer.sprite = GetSprite(type);
        renderer.color = type == PickupType.Heal
            ? new Color(0.96f, 0.3f, 0.36f, 1f)
            : new Color(0.26f, 0.8f, 1f, 1f);
        renderer.sortingOrder = 22;

        CircleCollider2D collider = pickup.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.42f;

        Rigidbody2D body = pickup.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.simulated = true;

        StagePickup stagePickup = pickup.AddComponent<StagePickup>();
        stagePickup.pickupType = type;
    }

    private void OnEnable()
    {
        if (!ActivePickups.Contains(this))
        {
            ActivePickups.Add(this);
        }
    }

    private void OnDisable()
    {
        ActivePickups.Remove(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        switch (pickupType)
        {
            case PickupType.Heal:
                Health health = collision.GetComponent<Health>();
                if (health != null)
                {
                    health.Heal(healAmount);
                }
                break;
            case PickupType.Magnet:
                ExperiencePickup.StartVacuumAll(collision.transform, 28f);
                break;
        }

        Destroy(gameObject);
    }

    private static Sprite GetSprite(PickupType type)
    {
        if (SpriteCache.TryGetValue(type, out Sprite sprite))
        {
            return sprite;
        }

        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        Color32[] pixels = new Color32[size * size];

        float center = (size - 1) * 0.5f;
        float radius = size * 0.28f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool draw = false;
                if (type == PickupType.Heal)
                {
                    draw = Mathf.Abs(x - center) <= 3f && Mathf.Abs(y - center) <= radius
                        || Mathf.Abs(y - center) <= 3f && Mathf.Abs(x - center) <= radius;
                }
                else
                {
                    float dx = x - center;
                    float dy = y - center;
                    draw = dx * dx + dy * dy <= radius * radius;
                    if (draw && x > center + 4f)
                    {
                        draw = false;
                    }
                }

                pixels[y * size + x] = draw ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        SpriteCache[type] = sprite;
        return sprite;
    }
}
