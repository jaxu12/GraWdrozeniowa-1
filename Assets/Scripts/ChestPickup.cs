using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ChestPickup : MonoBehaviour
{
    private static Sprite cachedSprite;

    public static void Create(Vector3 position)
    {
        GameObject chest = new GameObject("ChestPickup");
        chest.transform.position = new Vector3(position.x, position.y, 0f);

        SpriteRenderer renderer = chest.AddComponent<SpriteRenderer>();
        renderer.sprite = GetSprite();
        renderer.color = new Color(1f, 0.78f, 0.17f, 1f);
        renderer.sortingOrder = 21;

        BoxCollider2D collider = chest.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.9f, 0.9f);

        Rigidbody2D body = chest.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.simulated = true;

        chest.AddComponent<ChestPickup>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        GameManager.Instance?.OpenChestReward();
        Destroy(gameObject);
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

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool inside = x >= 5 && x <= 26 && y >= 7 && y <= 24;
                bool band = inside && y >= 14 && y <= 17;
                pixels[y * size + x] = inside
                    ? (band ? new Color32(120, 75, 15, 255) : new Color32(255, 255, 255, 255))
                    : new Color32(0, 0, 0, 0);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        cachedSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        return cachedSprite;
    }
}
