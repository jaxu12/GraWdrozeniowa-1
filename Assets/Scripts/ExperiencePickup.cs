using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ExperiencePickup : MonoBehaviour
{
    private static Sprite cachedSprite;
    private static readonly List<ExperiencePickup> ActivePickups = new List<ExperiencePickup>();

    [SerializeField] private int amount = 1;
    [SerializeField] private float magnetSpeed = 8f;

    private Transform player;
    private PlayerStatsRuntime playerStats;
    private float forcedMagnetSpeed;
    private bool forceVacuum;

    public static void Create(Vector3 position, int xpAmount)
    {
        GameObject pickup = new GameObject("ExperiencePickup");
        pickup.transform.position = new Vector3(position.x, position.y, 0f);

        SpriteRenderer renderer = pickup.AddComponent<SpriteRenderer>();
        renderer.sprite = GetSprite();
        renderer.color = new Color(0.3f, 1f, 0.5f, 1f);
        renderer.sortingOrder = 20;

        CircleCollider2D collider = pickup.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.35f;

        Rigidbody2D body = pickup.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.simulated = true;

        ExperiencePickup pickupComponent = pickup.AddComponent<ExperiencePickup>();
        pickupComponent.amount = xpAmount;
    }

    public static void StartVacuumAll(Transform target, float speed)
    {
        foreach (ExperiencePickup pickup in ActivePickups)
        {
            if (pickup == null)
            {
                continue;
            }

            pickup.player = target;
            pickup.forceVacuum = true;
            pickup.forcedMagnetSpeed = Mathf.Max(speed, pickup.magnetSpeed);
        }
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

    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        if (player == null || playerStats == null)
        {
            FindPlayer();
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);
        if (forceVacuum || distance <= playerStats.PickupRange)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                (forceVacuum ? forcedMagnetSpeed : magnetSpeed) * Time.deltaTime
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        GameManager.Instance?.AddExperience(amount);
        Destroy(gameObject);
    }

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            return;
        }

        player = playerObject.transform;
        playerStats = playerObject.GetComponent<PlayerStatsRuntime>();
    }

    private static Sprite GetSprite()
    {
        if (cachedSprite != null)
        {
            return cachedSprite;
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
                float dx = x - center;
                float dy = y - center;
                bool inside = dx * dx + dy * dy <= radius * radius;
                pixels[y * size + x] = inside ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        cachedSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        return cachedSprite;
    }
}
